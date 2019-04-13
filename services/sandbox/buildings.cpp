#include "buildings.h"
#include "tiny_obj_loader.h"
#include <vector>
#include <random>


const char* kModelName = "models/building.obj";
const uint32_t kVertexSize = 3 * sizeof(float);


bool Buildings::Init(uint32_t fieldSizeX, uint32_t fieldSizeY)
{
	m_fieldSizeX = fieldSizeX;
	m_fieldSizeY = fieldSizeY;

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
	
	glGenVertexArrays(1, &m_vao);
	glGenBuffers(1, &m_vbo);
	glGenBuffers(1, &m_ebo);

	glBindVertexArray(m_vao);

	glBindBuffer(GL_ARRAY_BUFFER, m_vbo);
	glBufferData(GL_ARRAY_BUFFER, attrib.vertices.size() * sizeof(float), attrib.vertices.data(), GL_STATIC_DRAW);

	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, m_ebo);
	m_indicesNum = shapes[0].mesh.indices.size();
	uint16_t* indices = new uint16_t[m_indicesNum];
	for (uint32_t i = 0; i < m_indicesNum; i++)
		indices[i] = shapes[0].mesh.indices[i].vertex_index;
	glBufferData(GL_ELEMENT_ARRAY_BUFFER, m_indicesNum * sizeof(uint16_t), indices, GL_STATIC_DRAW);
	delete[] indices;

	glEnableVertexAttribArray(0);
	glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, kVertexSize, (void*)0);

	glBindVertexArray(0);

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

	m_numBuildingsX = fieldSizeX / (kBuildingSize + kStreetWidth);
	m_numBuildingsY = fieldSizeY / (kBuildingSize + kStreetWidth);

	GenerateBuildings();

	return true;
}


void Buildings::Shutdown()
{
	if (m_ebo)
	{
		glDeleteBuffers(1, &m_ebo);
		m_ebo = 0;
	}
	if (m_vbo)
	{
		glDeleteBuffers(1, &m_vbo);
		m_vbo = 0;
	}
	if (m_vao)
	{
		glDeleteVertexArrays(1, &m_vao);
		m_vao = 0;
	}
	m_indicesNum = 0;

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

	m_buildings.clear();
}


void Buildings::Draw(const glm::mat4& viewProjMatrix, const glm::vec3& viewDir)
{
	if (!m_program)
		return;

	glBindVertexArray(m_vao);

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

	glDrawElementsInstanced(GL_TRIANGLES, m_indicesNum, GL_UNSIGNED_SHORT, 0, m_numBuildingsX * m_numBuildingsY);

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