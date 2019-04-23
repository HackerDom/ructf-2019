#include "buildings.h"
#include "tiny_obj_loader.h"
#include <vector>
#include <random>


const char* kModelName = "models/building.obj";
const char* kTextureName = "models/grid.png";

struct Vertex
{
	float pos[3];
	float uv[2];
};


bool Buildings::Init(uint32_t fieldSizeX, uint32_t fieldSizeY)
{
	m_fieldSizeX = fieldSizeX;
	m_fieldSizeY = fieldSizeY;

	m_numBuildingsX = fieldSizeX / (kBuildingSize + kStreetWidth);
	m_numBuildingsY = fieldSizeY / (kBuildingSize + kStreetWidth);

	{
		tinyobj::attrib_t attrib;
		std::vector<tinyobj::shape_t> shapes;
		std::vector<tinyobj::material_t> materials;
		std::string warn;
		std::string err;
		if (!tinyobj::LoadObj(&attrib, &shapes, &materials, &warn, &err, kModelName))
		{
			printf("Failed to load building model\n");
			return false;
		}

		glGenVertexArrays(1, &m_mesh.vao);
		glGenBuffers(1, &m_mesh.vertexVbo);
		glGenBuffers(1, &m_mesh.instancesVbo);

		glBindVertexArray(m_mesh.vao);

		glBindBuffer(GL_ARRAY_BUFFER, m_mesh.vertexVbo);
		m_verticesNum = shapes[0].mesh.indices.size();
		Vertex* vertices = new Vertex[m_verticesNum];
		for (uint32_t i = 0; i < m_verticesNum; i++)
		{
			auto idx = shapes[0].mesh.indices[i];
			
			for(uint32_t j = 0; j < 3; j++)
				vertices[i].pos[j] = attrib.vertices[3 * idx.vertex_index + j];

			for (uint32_t j = 0; j < 2; j++)
				vertices[i].uv[j] = attrib.texcoords[2 * idx.texcoord_index + j];
		}
		glBufferData(GL_ARRAY_BUFFER, m_verticesNum * sizeof(Vertex), vertices, GL_STATIC_DRAW);
		delete[] vertices;

		glEnableVertexAttribArray(0);
		glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, sizeof(Vertex), (void*)0);
		glEnableVertexAttribArray(1);
		glVertexAttribPointer(1, 2, GL_FLOAT, GL_FALSE, sizeof(Vertex), (void*)offsetof(Vertex, uv));

		glBindBuffer(GL_ARRAY_BUFFER, m_mesh.instancesVbo);
		glBufferData(GL_ARRAY_BUFFER, m_numBuildingsX * m_numBuildingsY * sizeof(uint32_t), nullptr, GL_DYNAMIC_DRAW);
		glEnableVertexAttribArray(2);
		glVertexAttribIPointer(2, 1, GL_UNSIGNED_INT, sizeof(uint32_t), (void*)0);
		glVertexAttribDivisor(2, 1);

		glBindVertexArray(0);

		glBindBuffer(GL_SHADER_STORAGE_BUFFER, m_mesh.instancesVbo);
		glBindBuffer(GL_SHADER_STORAGE_BUFFER, 0);

		glGenBuffers(1, &m_indirectBuffer);
		glBindBuffer(GL_ATOMIC_COUNTER_BUFFER, m_indirectBuffer);
		DrawArraysIndirectCommand initialCmd;
		initialCmd.count = m_verticesNum;
		initialCmd.primCount = 0;
		initialCmd.first = 0;
		initialCmd.baseInstance = 0;
		glBufferData(GL_ATOMIC_COUNTER_BUFFER, sizeof(DrawArraysIndirectCommand), &initialCmd, GL_DYNAMIC_DRAW);
		glBindBuffer(GL_ATOMIC_COUNTER_BUFFER, 0);

		glBindBuffer(GL_DRAW_INDIRECT_BUFFER, m_indirectBuffer);
		glBindBuffer(GL_DRAW_INDIRECT_BUFFER, 0);
	}

	m_vs = new VertexShader("shaders/building.vert");
	if (!m_vs->IsValid())
		return false;
	m_fs = new FragmentShader("shaders/building.frag");
	if (!m_fs->IsValid())
		return false;

	Shader* colorShaders[] = {m_vs, m_fs};
	m_program = new Program(colorShaders, 2);
	if (!m_program->IsValid())
		return false;

	m_visCs = new ComputeShader("shaders/building_visibility.cs");
	if (!m_visCs->IsValid())
		return false;
	Shader* visShaders[] = { m_visCs };
	m_visProgram = new Program(visShaders, 1);
	if (!m_visProgram->IsValid())
		return false;

#if HAS_LIBPNG
	Image image;
	if (!read_png(kTextureName, image))
		return false;

	m_texture = new Texture2D(image);

	glBindTexture(GL_TEXTURE_2D, m_texture->GetTexture());
	glGenerateTextureMipmap(m_texture->GetTexture());
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR_MIPMAP_LINEAR);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
	glBindTexture(GL_TEXTURE_2D, 0);
#else
	uint32_t initData[4 * 4];
	memset(initData, 0xff, sizeof(initData));
	m_texture = new Texture2D(4, 4, FORMAT_RGBA8, initData);
#endif
	GenerateBuildings();

	return true;
}


void Buildings::Shutdown()
{
	DeleteBuffer(m_mesh.vertexVbo);
	DeleteBuffer(m_mesh.instancesVbo);
	if (m_mesh.vao)
	{
		glDeleteVertexArrays(1, &m_mesh.vao);
		m_mesh.vao = 0;
	}

	if (m_program)
	{
		delete m_program;
		m_program = nullptr;
	}
	if (m_fs)
	{
		delete m_fs;
		m_fs = nullptr;
	}
	if (m_vs)
	{
		delete m_vs;
		m_vs = nullptr;
	}

	if (m_visProgram)
	{
		delete m_visProgram;
		m_visProgram = nullptr;
	}
	if (m_visCs)
	{
		delete m_visCs;
		m_visCs = nullptr;
	}
	DeleteBuffer(m_indirectBuffer);

	if (m_texture)
		delete m_texture;
}


void Buildings::Draw(const glm::mat4& projMatrix, GLuint cameraDataSsbo)
{
	if (!m_program)
		return;

	{
		glBindBuffer(GL_ATOMIC_COUNTER_BUFFER, m_indirectBuffer);
		DrawArraysIndirectCommand initialCmd;
		initialCmd.count = m_verticesNum;
		initialCmd.primCount = 0;
		initialCmd.first = 0;
		initialCmd.baseInstance = 0;
		glBufferSubData(GL_ATOMIC_COUNTER_BUFFER, 0, sizeof(DrawArraysIndirectCommand), &initialCmd);
		glBindBuffer(GL_ATOMIC_COUNTER_BUFFER, 0);
	}

	glUseProgram(m_visProgram->GetProgram());
	m_visProgram->SetIVec4("numBuildings", glm::ivec4(m_numBuildingsX, m_numBuildingsY, 0, 0));
	m_visProgram->SetVec4("buildingSize", glm::vec4(kBuildingSize, kStreetWidth, 0.0f, 0.0f));
	m_visProgram->SetSSBO("CameraData", cameraDataSsbo);
	glBindBufferBase(GL_ATOMIC_COUNTER_BUFFER, 0, m_indirectBuffer);
	glBindBufferBase(GL_SHADER_STORAGE_BUFFER, 0, m_mesh.instancesVbo);
	m_visProgram->BindUniforms();
	glDispatchCompute(m_numBuildingsX / 8, m_numBuildingsY / 8, 1);
	glMemoryBarrier(GL_VERTEX_ATTRIB_ARRAY_BARRIER_BIT | GL_ATOMIC_COUNTER_BARRIER_BIT);

	glUseProgram(m_program->GetProgram());
	m_program->SetMat4("projMatrix", projMatrix);
	m_program->SetIVec4("numBuildings", glm::ivec4(m_numBuildingsX, m_numBuildingsY, 0, 0));
	m_program->SetVec4("buildingSize", glm::vec4(kBuildingSize, kStreetWidth, 0.0f, 0.0f));
	m_program->SetSSBO("CameraData", cameraDataSsbo);
	m_program->SetTexture("tex", *m_texture);
	m_program->BindUniforms();

	glEnable(GL_DEPTH_TEST);
	glDepthFunc(GL_LESS);
	glEnable(GL_CULL_FACE);
	glCullFace(GL_FRONT);
	glFrontFace(GL_CW);

	{
		glBindVertexArray(m_mesh.vao);
		glBindBuffer(GL_DRAW_INDIRECT_BUFFER, m_indirectBuffer);
		glDrawArraysIndirect(GL_TRIANGLES, (void*)0);
		glBindBuffer(GL_DRAW_INDIRECT_BUFFER, 0);
	}

	glBindVertexArray(0);
}


void Buildings::GenerateBuildings()
{
	std::default_random_engine e;
	std::uniform_real_distribution<> disColor(0.0, 0.8);

	const float floatStreetLength = kStreetWidth;

	float newBuildingLeft = floatStreetLength;
	float newBuildingTop = floatStreetLength;

	while (1)
	{
		Building b;

		b.sizeX = kBuildingSize;
		b.sizeY = kBuildingSize;
		b.color[0] = (float)disColor(e);
		b.color[1] = (float)disColor(e);
		b.color[2] = (float)disColor(e);
		b.color[3] = 0.0f;

		if (newBuildingLeft + b.sizeX > (float)m_fieldSizeX)
		{
			newBuildingLeft = floatStreetLength;
			newBuildingTop += b.sizeY + floatStreetLength;
			if (newBuildingTop > (float)m_fieldSizeY)
				break;
		}

		b.posX = newBuildingLeft + b.sizeX * 0.5f;
		b.posY = newBuildingTop + b.sizeY * 0.5f;

		newBuildingLeft += b.sizeX + floatStreetLength;
	}
}
