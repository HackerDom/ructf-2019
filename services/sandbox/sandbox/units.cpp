#include "units.h"
#include "buildings.h"
#include <random>


bool Units::Init(uint32_t fieldSizeX, uint32_t fieldSizeY)
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
		return false;

	glGenVertexArrays(1, &m_vao);
	glGenBuffers(1, &m_instancesVbo);

	glBindVertexArray(m_vao);

	glBindBuffer(GL_ARRAY_BUFFER, m_instancesVbo);
	glBufferData(GL_ARRAY_BUFFER, kMaxUnitsCount * sizeof(uint32_t), nullptr, GL_DYNAMIC_DRAW);
	glEnableVertexAttribArray(0);
	glVertexAttribIPointer(0, 1, GL_UNSIGNED_INT, sizeof(uint32_t), (void*)0);
	glVertexAttribDivisor(0, 1);

	glBindVertexArray(0);

	glBindBuffer(GL_SHADER_STORAGE_BUFFER, m_instancesVbo);
	glBindBuffer(GL_SHADER_STORAGE_BUFFER, 0); // unbind

	glGenBuffers(1, &m_ssbo);
	glBindBuffer(GL_SHADER_STORAGE_BUFFER, m_ssbo);
	glBufferData(GL_SHADER_STORAGE_BUFFER, kMaxUnitsCount * sizeof(Unit), nullptr, GL_DYNAMIC_DRAW);
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

	m_fieldSizeX = fieldSizeX;
	m_fieldSizeY = fieldSizeY;
	m_units.reserve(kMaxUnitsCount);

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


uint32_t Units::AddUnit(uint32_t mind[8], float power)
{
	if (m_units.size() >= kMaxUnitsCount)
	{
		printf("Too much units in simulation\n");
		return ~0u;
	}

	std::default_random_engine e;
	std::uniform_real_distribution<> dis(0.1, 1.0);
	std::uniform_real_distribution<> disPos(-16.0, 16.0);

	Unit u;
	memcpy(u.mind, mind, 32);

	while (1)
	{
		u.id = rand();
		if (m_idToIdx.find(u.id) == m_idToIdx.end())
			break;
	}

	u.posX = (float)(m_fieldSizeX - kStreetWidth) * 0.5f + (float)kStreetWidth * 0.5 + (float)dis(e);
	u.posY = (float)disPos(e);
	u.posZ = (float)(m_fieldSizeY - kStreetWidth) * 0.5f + (float)kStreetWidth * 0.5 + (float)dis(e);

	u.type = kUnitHuman;
	u.power = power;
	u.prevDirIdx = 0;
	u.prevCrossIdx = 0;

	m_unitsToAdd.push_back(u);

	return u.id;
}


const Unit* Units::GetUnit(uint32_t id)
{
	if(m_idToIdx.find(id) == m_idToIdx.end())
		return nullptr;

	uint32_t idx = m_idToIdx[id];
	return &m_units[idx];
}


void Units::AddPendingUnits()
{
	for (auto& u : m_unitsToAdd)
	{
		uint32_t idx = m_units.size();
		m_idToIdx[u.id] = idx;
		m_units.push_back(u);
	}
	m_unitsToAdd.clear();
}


void Units::Simulate(const Texture2D& target, const Texture2D& randomTex)
{
	if (!m_simulationProgram)
		return;

	glBindBuffer(GL_SHADER_STORAGE_BUFFER, m_ssbo);
	if(m_units.size())
		glGetBufferSubData(GL_SHADER_STORAGE_BUFFER, 0, sizeof(Unit) * m_units.size(), m_units.data());

	AddPendingUnits();
	if (m_units.size())
		glBufferSubData(GL_SHADER_STORAGE_BUFFER, 0, sizeof(Unit) * m_units.size(), m_units.data());
	glBindBuffer(GL_SHADER_STORAGE_BUFFER, 0);

	if (m_units.size())
	{
		glUseProgram(m_simulationProgram->GetProgram());
		m_simulationProgram->SetImage("simulationTex", target, GL_WRITE_ONLY);
		m_simulationProgram->SetIVec4("unitsCount", glm::ivec4(m_units.size(), 0, 0, 0));
		m_simulationProgram->SetTexture("randomTex", randomTex);
		m_simulationProgram->SetSSBO("Units", m_ssbo);
		m_simulationProgram->BindUniforms();
		glDispatchCompute((m_units.size() + 31) / 32, 1, 1);

		glMemoryBarrier(GL_SHADER_IMAGE_ACCESS_BARRIER_BIT | GL_SHADER_STORAGE_BARRIER_BIT);
	}
}


void Units::Draw(const glm::mat4& viewProjMatrix, const glm::mat4& viewMatrix, const glm::vec4 frustumPlanes[])
{
	if (!m_visualizationProgram)
		return;

	if (m_units.empty())
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
