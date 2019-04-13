#include "buildings.h"
#include "tiny_obj_loader.h"
#include <vector>
#include <random>


const char* kModelsName[] = { "models/building_lod0.obj", "models/building_lod1.obj", "models/building_lod2.obj", "models/building_lod3.obj" };
const uint32_t kVertexSize = 3 * sizeof(float);


bool Buildings::Init(uint32_t fieldSizeX, uint32_t fieldSizeY)
{
	m_fieldSizeX = fieldSizeX;
	m_fieldSizeY = fieldSizeY;

	m_numBuildingsX = fieldSizeX / (kBuildingSize + kStreetWidth);
	m_numBuildingsY = fieldSizeY / (kBuildingSize + kStreetWidth);

	for (uint32_t l = 0; l < kLodsCount; l++)
	{
		tinyobj::attrib_t attrib;
		std::vector<tinyobj::shape_t> shapes;
		std::vector<tinyobj::material_t> materials;
		std::string warn;
		std::string err;
		if (!tinyobj::LoadObj(&attrib, &shapes, &materials, &warn, &err, kModelsName[l]))
		{
			printf("Failed to load building model\n");
			return false;
		}

		glGenVertexArrays(1, &m_meshes[l].vao);
		glGenBuffers(1, &m_meshes[l].positionsVbo);
		glGenBuffers(1, &m_meshes[l].instancesVbo);
		glGenBuffers(1, &m_meshes[l].indicesBuffer);

		glBindVertexArray(m_meshes[l].vao);

		glBindBuffer(GL_ARRAY_BUFFER, m_meshes[l].positionsVbo);
		glBufferData(GL_ARRAY_BUFFER, attrib.vertices.size() * sizeof(float), attrib.vertices.data(), GL_STATIC_DRAW);
		glEnableVertexAttribArray(0);
		glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, kVertexSize, (void*)0);

		glBindBuffer(GL_ARRAY_BUFFER, m_meshes[l].instancesVbo);
		glBufferData(GL_ARRAY_BUFFER, m_numBuildingsX * m_numBuildingsY * sizeof(uint32_t), nullptr, GL_DYNAMIC_DRAW);
		glEnableVertexAttribArray(1);
		glVertexAttribIPointer(1, 1, GL_UNSIGNED_INT, sizeof(uint32_t), (void*)0);
		glVertexAttribDivisor(1, 1);

		glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, m_meshes[l].indicesBuffer);
		m_lodIndicesNum[l] = shapes[0].mesh.indices.size();
		uint16_t* indices = new uint16_t[m_lodIndicesNum[l]];
		for (uint32_t i = 0; i < m_lodIndicesNum[l]; i++)
			indices[i] = shapes[0].mesh.indices[i].vertex_index;
		glBufferData(GL_ELEMENT_ARRAY_BUFFER, m_lodIndicesNum[l] * sizeof(uint16_t), indices, GL_STATIC_DRAW);
		delete[] indices;

		glBindVertexArray(0);

		glBindBuffer(GL_SHADER_STORAGE_BUFFER, m_meshes[l].instancesVbo);
		glBindBuffer(GL_SHADER_STORAGE_BUFFER, 0); // unbind

		glGenBuffers(1, &m_indirectBuffer[l]);
		glBindBuffer(GL_ATOMIC_COUNTER_BUFFER, m_indirectBuffer[l]);
		DrawElementsIndirectCommand initialCmd;
		initialCmd.count = m_lodIndicesNum[l];
		initialCmd.primCount = 0;
		initialCmd.firstIndex = 0;
		initialCmd.baseVertex = 0;
		initialCmd.baseInstance = 0;
		glBufferData(GL_ATOMIC_COUNTER_BUFFER, sizeof(DrawElementsIndirectCommand), &initialCmd, GL_DYNAMIC_DRAW);
		glBindBuffer(GL_ATOMIC_COUNTER_BUFFER, 0);

		glBindBuffer(GL_DRAW_INDIRECT_BUFFER, m_indirectBuffer[l]);
		glBindBuffer(GL_DRAW_INDIRECT_BUFFER, 0);
	}

	m_vs = new VertexShader("shaders/building.vert");
	if (!m_vs->IsValid())
		return false;
	m_gs = new GeometryShader("shaders/building.geom");
	if (!m_gs->IsValid())
		return false;
	m_fs = new FragmentShader("shaders/building.frag");
	if (!m_fs->IsValid())
		return false;

	Shader* colorShaders[] = {m_vs, m_gs, m_fs};
	m_program = new Program(colorShaders, 3);
	if (!m_program->IsValid())
		return false;

	m_visCs = new ComputeShader("shaders/building_visibility.cs");
	if (!m_visCs->IsValid())
		return false;
	Shader* visShaders[] = { m_visCs };
	m_visProgram = new Program(visShaders, 1);
	if (!m_visProgram->IsValid())
		return false;

	GenerateBuildings();

	return true;
}


static void DeleteBuffer(GLuint& b)
{
	if (b)
	{
		glDeleteBuffers(1, &b);
		b = 0;
	}
}


void Buildings::Shutdown()
{
	for (uint32_t l = 0; l < kLodsCount; l++)
	{
		DeleteBuffer(m_meshes[l].positionsVbo);
		DeleteBuffer(m_meshes[l].instancesVbo);
		DeleteBuffer(m_meshes[l].indicesBuffer);
		if (m_meshes[l].vao)
		{
			glDeleteVertexArrays(1, &m_meshes[l].vao);
			m_meshes[l].vao = 0;
		}
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
	if (m_gs)
	{
		delete m_gs;
		m_gs = nullptr;
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
	for (uint32_t l = 0; l < kLodsCount; l++)
		DeleteBuffer(m_indirectBuffer[l]);

	m_buildings.clear();
}


void Buildings::Draw(const glm::mat4& viewProjMatrix, const glm::vec3& viewDir, const glm::vec3& viewerPos, const glm::vec4 frustumPlanes[])
{
	if (!m_program)
		return;

	for (uint32_t l = 0; l < kLodsCount; l++)
	{
		glBindBuffer(GL_ATOMIC_COUNTER_BUFFER, m_indirectBuffer[l]);
		DrawElementsIndirectCommand initialCmd;
		initialCmd.count = m_lodIndicesNum[l];
		initialCmd.primCount = 0;
		initialCmd.firstIndex = 0;
		initialCmd.baseVertex = 0;
		initialCmd.baseInstance = 0;
		glBufferSubData(GL_ATOMIC_COUNTER_BUFFER, 0, sizeof(DrawElementsIndirectCommand), &initialCmd);
		glBindBuffer(GL_ATOMIC_COUNTER_BUFFER, 0);
	}

	glUseProgram(m_visProgram->GetProgram());
	m_visProgram->SetIVec4("numBuildings", glm::ivec4(m_numBuildingsX, m_numBuildingsY, 0, 0));
	m_visProgram->SetVec4("buildingSize", glm::vec4(kBuildingSize, kStreetWidth, 0.0f, 0.0f));
	m_visProgram->SetVec4("viewerPos",  glm::vec4(viewerPos.x, viewerPos.y, viewerPos.z, 0.0f));
	for (uint32_t l = 0; l < kLodsCount; l++)
	{
		glBindBufferBase(GL_ATOMIC_COUNTER_BUFFER, l, m_indirectBuffer[l]);
		glBindBufferBase(GL_SHADER_STORAGE_BUFFER, l, m_meshes[l].instancesVbo);
	}
	m_visProgram->BindUniforms();
	GLint location = glGetUniformLocation(m_visProgram->GetProgram(), "frustumPlanes");
	glUniform4fv(location, 6, (GLfloat*)frustumPlanes);
	glDispatchCompute(m_numBuildingsX / 8, m_numBuildingsY / 8, 1);
	glMemoryBarrier(GL_VERTEX_ATTRIB_ARRAY_BARRIER_BIT | GL_ATOMIC_COUNTER_BARRIER_BIT);

	glUseProgram(m_program->GetProgram());
	m_program->SetMat4("viewProjMatrix", viewProjMatrix);
	m_program->SetVec4("viewDir", glm::vec4(viewDir.x, viewDir.y, viewDir.z, 0.0f));
	m_program->SetIVec4("numBuildings", glm::ivec4(m_numBuildingsX, m_numBuildingsY, 0, 0));
	m_program->SetVec4("buildingSize", glm::vec4(kBuildingSize, kStreetWidth, 0.0f, 0.0f));
	m_program->BindUniforms();

	glEnable(GL_DEPTH_TEST);
	glDepthFunc(GL_LESS);
	glEnable(GL_CULL_FACE);
	glCullFace(GL_FRONT);
	glFrontFace(GL_CW);

	for (uint32_t l = 0; l < kLodsCount; l++)
	{
		glBindVertexArray(m_meshes[l].vao);
		glBindBuffer(GL_DRAW_INDIRECT_BUFFER, m_indirectBuffer[l]);
		glDrawElementsIndirect(GL_TRIANGLES, GL_UNSIGNED_SHORT, (void*)0);
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

		m_buildings.push_back(b);
	}
}