#include "units.h"
#include "buildings.h"
#include <random>


bool Units::Init(uint32_t numUnits, uint32_t fieldSizeX, uint32_t fieldSizeY)
{
	m_visualizationVs = new VertexShader("shaders/unit.vert");
	if (!m_visualizationVs->IsValid())
		return false;
	m_visualizationFs = new FragmentShader("shaders/unit.frag");
	if (!m_visualizationFs->IsValid())
		return false;

	Shader* colorShaders[] = {m_visualizationVs, m_visualizationFs};
	m_visualizationProgram = new Program(colorShaders, 2);
	if (!m_visualizationProgram->IsValid())
		return false;

	m_visibilityCs = new ComputeShader("shaders/unit_visibility.cs");
	if (!m_visibilityCs->IsValid())
		return false;
	Shader* visShaders[] = { m_visibilityCs };
	m_visibilityProgram = new Program(visShaders, 1);
	if (!m_visibilityProgram->IsValid())
		return false;

	m_simulationCs = new ComputeShader("shaders/unit_simulation.cs");
	Shader* csArray[] = {m_simulationCs};
	m_simulationProgram = new Program(csArray, 1);
	if (!m_simulationProgram->IsValid())
		return 1;

	GenerateUnits(numUnits, fieldSizeX, fieldSizeY);

	glGenVertexArrays(1, &m_vao);
	glGenBuffers(1, &m_instancesVbo);

	glBindVertexArray(m_vao);

	glBindBuffer(GL_ARRAY_BUFFER, m_instancesVbo);
	glBufferData(GL_ARRAY_BUFFER, numUnits * sizeof(uint32_t), nullptr, GL_DYNAMIC_DRAW);
	glEnableVertexAttribArray(0);
	glVertexAttribIPointer(0, 1, GL_UNSIGNED_INT, sizeof(uint32_t), (void*)0);
	glVertexAttribDivisor(0, 1);

	glBindVertexArray(0);

	glBindBuffer(GL_SHADER_STORAGE_BUFFER, m_instancesVbo);
	glBindBuffer(GL_SHADER_STORAGE_BUFFER, 0); // unbind

	glGenBuffers(1, &m_ssbo);
	glBindBuffer(GL_SHADER_STORAGE_BUFFER, m_ssbo);
	glBufferData(GL_SHADER_STORAGE_BUFFER, m_units.size() * sizeof(Unit), m_units.data(), GL_DYNAMIC_DRAW);
	glBindBuffer(GL_SHADER_STORAGE_BUFFER, 0); // unbind

	glGenBuffers(1, &m_indirectBuffer);
	glBindBuffer(GL_ATOMIC_COUNTER_BUFFER, m_indirectBuffer);
	DrawArraysIndirectCommand initialCmd;
	initialCmd.count = 6;
	initialCmd.primCount = 0;
	initialCmd.first = 0;
	initialCmd.baseInstance = 0;
	glBufferData(GL_ATOMIC_COUNTER_BUFFER, sizeof(DrawArraysIndirectCommand), &initialCmd, GL_DYNAMIC_DRAW);
	glBindBuffer(GL_ATOMIC_COUNTER_BUFFER, 0);

	glBindBuffer(GL_DRAW_INDIRECT_BUFFER, m_indirectBuffer);
	glBindBuffer(GL_DRAW_INDIRECT_BUFFER, 0);

	return true;
}


void Units::Shutdown()
{
	if (m_vao)
	{
		glDeleteVertexArrays(1, &m_vao);
		m_vao = 0;
	}
	DeleteBuffer(m_instancesVbo);
	DeleteBuffer(m_indirectBuffer);

	if (m_visualizationProgram)
	{
		delete m_visualizationProgram;
		m_visualizationProgram = nullptr;
	}
	if (m_visualizationFs)
	{
		delete m_visualizationFs;
		m_visualizationFs = nullptr;
	}
	if (m_visualizationVs)
	{
		delete m_visualizationVs;
		m_visualizationVs = nullptr;
	}

	if (m_visibilityProgram)
	{
		delete m_visibilityProgram;
		m_visibilityProgram = 0;
	}
	if (m_visibilityCs)
	{
		delete m_visibilityCs;
		m_visibilityCs = nullptr;
	}

	if (m_simulationProgram)
	{
		delete m_simulationProgram;
		m_simulationProgram = 0;
	}
	if (m_simulationCs)
	{
		delete m_simulationCs;
		m_simulationCs = nullptr;
	}
}


void Units::Simulate(const Texture2D& target, const Texture2D& randomTex)
{
	if (!m_simulationProgram)
		return;

	glUseProgram(m_simulationProgram->GetProgram());
	m_simulationProgram->SetImage("img_output", target, GL_WRITE_ONLY);
	m_simulationProgram->SetIVec4("unitsCount", glm::ivec4(m_units.size(), 0, 0, 0));
	m_simulationProgram->SetTexture("randomTex", randomTex);
	m_simulationProgram->SetSSBO("Units", m_ssbo);
	m_simulationProgram->BindUniforms();
	glDispatchCompute((m_units.size() + 31) / 32, 1, 1);

	glMemoryBarrier(GL_SHADER_IMAGE_ACCESS_BARRIER_BIT | GL_SHADER_STORAGE_BARRIER_BIT);
}


void Units::Draw(const glm::mat4& viewProjMatrix, const glm::mat4& viewMatrix, const glm::vec4 frustumPlanes[])
{
	if (!m_visualizationProgram)
		return;

	glBindBuffer(GL_ATOMIC_COUNTER_BUFFER, m_indirectBuffer);
	DrawArraysIndirectCommand initialCmd;
	initialCmd.count = 6;
	initialCmd.primCount = 0;
	initialCmd.first = 0;
	initialCmd.baseInstance = 0;
	glBufferSubData(GL_ATOMIC_COUNTER_BUFFER, 0, sizeof(DrawArraysIndirectCommand), &initialCmd);
	glBindBuffer(GL_ATOMIC_COUNTER_BUFFER, 0);

	glUseProgram(m_visibilityProgram->GetProgram());
	glBindBufferBase(GL_ATOMIC_COUNTER_BUFFER, 0, m_indirectBuffer);
	glBindBufferBase(GL_SHADER_STORAGE_BUFFER, 0, m_instancesVbo);
	glBindBufferBase(GL_SHADER_STORAGE_BUFFER, 1, m_ssbo);
	GLint location = glGetUniformLocation(m_visibilityProgram->GetProgram(), "frustumPlanes");
	glUniform4fv(location, 6, (GLfloat*)frustumPlanes);
	m_visibilityProgram->SetIVec4("unitsCount", glm::ivec4(m_units.size(), 0, 0, 0));
	m_visibilityProgram->BindUniforms();
	glDispatchCompute((m_units.size() + 31) / 32, 1, 1);
	glMemoryBarrier(GL_VERTEX_ATTRIB_ARRAY_BARRIER_BIT | GL_ATOMIC_COUNTER_BARRIER_BIT);

	glBindVertexArray(m_vao);

	glUseProgram(m_visualizationProgram->GetProgram());
	m_visualizationProgram->SetMat4("viewProjMatrix", viewProjMatrix);
	m_visualizationProgram->SetSSBO("Units", m_ssbo);
	m_visualizationProgram->SetMat4("viewMatrix", viewMatrix);
	m_visualizationProgram->BindUniforms();

	glEnable(GL_DEPTH_TEST);
	glDepthFunc(GL_LESS);
	glEnable(GL_CULL_FACE);
	glCullFace(GL_FRONT);
	glFrontFace(GL_CW);

	glBindBuffer(GL_DRAW_INDIRECT_BUFFER, m_indirectBuffer);
	glDrawArraysIndirect(GL_TRIANGLES, (void*)0);

	glBindVertexArray(0);
}


void Units::GenerateUnits(uint32_t numUnits, uint32_t fieldSizeX, uint32_t fieldSizeY)
{
	std::default_random_engine e;
	std::uniform_real_distribution<> dis(0.1, 1.0);
	std::uniform_real_distribution<> disPos(-16.0, 16.0);

	m_units.reserve(numUnits);

	for (uint32_t i = 0; i < numUnits; i++)
	{
		Unit u;

		char mind[32];
		for (uint32_t j = 0; j < 32; j++)
			mind[j] = rand() % 256;
		memcpy(u.mind, mind, 32);

		u.id = rand();

		u.posX = (float)(fieldSizeX - kStreetWidth) * 0.5f + 4.0f + (float)dis(e);
		u.posY = (float)disPos(e);
		u.posZ = (float)(fieldSizeY - kStreetWidth) * 0.5f + 4.0f + (float)dis(e);

		u.type = rand() % 2;
		u.power = (float)dis(e);
		u.prevDirIdx = 0;
		u.prevCrossIdx = 0;

		m_units.push_back(u);
	}
}