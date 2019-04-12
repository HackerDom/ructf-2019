#include "buildings.h"
#include "tiny_obj_loader.h"
#include <vector>

const char* kModelName = "models/building.obj";
const uint32_t kVertexSize = 3 * sizeof(float);


bool Buildings::Init(uint32_t fieldSizeX, uint32_t fieldSizeY)
{
	uint32_t numBuildingsX = fieldSizeX / (kBuildingSize + kStreetWidth);
	uint32_t numBuildingsY = fieldSizeY / (kBuildingSize + kStreetWidth);

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
	m_fs = new FragmentShader("shaders/building.frag");
	if (!m_fs->IsValid())
		return false;
	Shader* shaders[] = {m_vs, m_fs};
	m_program = new Program(shaders, 2);
	if (!m_program->IsValid())
		return false;

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

	m_indicesNum = 0;
}


void Buildings::Draw(const glm::mat4& viewProjMatrix)
{
	if (!m_program)
		return;

	glUseProgram(m_program->GetProgram());
	m_program->SetMat4("viewProjMatrix", viewProjMatrix);
	m_program->BindUniforms();

	glBindVertexArray(m_vao);
	glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);
	glDrawElements(GL_TRIANGLES, m_indicesNum, GL_UNSIGNED_SHORT, 0);
	glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
	glBindVertexArray(0);
}