#include "units.h"
#include "buildings.h"
#include "thread_affinity.h"
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

	m_storage = fopen("storage.dat", "rb+");
	if(m_storage)
	{
		fseek(m_storage, 0, SEEK_END);
		size_t fileSize = ftell(m_storage);
		fseek(m_storage, 0, SEEK_SET);

		UUID uuid;
		size_t recordSize = uuid.size() + sizeof(Unit);
		if(fileSize % recordSize != 0)
		{
			printf("Storage corrupted\n");
		}
		else
		{
			size_t recordsNum = fileSize / recordSize;
			for(size_t i = 0; i < recordsNum; i++)
			{
				UUID uuid;
				if(fread(uuid.data(), uuid.size(), 1, m_storage) != 1)
				{
					printf("Failed to read storage\n");
					break;
				}

				Unit unit;
				if(fread(&unit, sizeof(Unit), 1, m_storage) != 1)
				{

					printf("Failed to read storage\n");
					break;
				}

				uint32_t idx = m_units.size();
				m_units.push_back(unit);
				m_uuidToIdx[uuid] = idx;
			}
		}

		if(m_units.size())
		{
			glBindBuffer(GL_SHADER_STORAGE_BUFFER, m_ssbo);
			glBufferSubData(GL_SHADER_STORAGE_BUFFER, 0, sizeof(Unit) * m_units.size(), m_units.data());
			glBindBuffer(GL_SHADER_STORAGE_BUFFER, 0);
		}

		printf("Units restored from storage: %u\n", m_units.size());
	}
	else
	{
		FILE* c = fopen("storage.dat", "w");
		fclose(c);
		m_storage = fopen("storage.dat", "rb+");
	}

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
	
	fflush(m_storage);
	fclose(m_storage);
}


Units::EAddResult Units::AddUnit(const UUID& uuid, uint32_t mind[8])
{
	std::lock_guard<std::mutex> lck(m_mutex);

	if (m_uuidToIdx.size() + m_unitsToAdd.size() >= kMaxUnitsCount)
	{
		printf("Too much units in simulation\n");
		return kAddTooMuchUnits;
	}

	if(m_uuidToIdx.find(uuid) != m_uuidToIdx.end())
	{
		char str[64] = {};
		uuid_unparse(uuid, str);
		printf("Unit %s already exists\n", str);
		return kAddAlreadyExists;
	}

	static std::default_random_engine e;
	static std::uniform_real_distribution<> dis(0.1, 1.0);
	static std::uniform_real_distribution<> disPos(-16.0, 16.0);

	Unit u;
	memcpy(u.mind, mind, 32);
	u.index = ~0u;

	uint32_t crossesPerX = m_fieldSizeX / (kStreetWidth + kBuildingSize);
	uint32_t crossesPerY = m_fieldSizeY / (kStreetWidth + kBuildingSize);
	u.posX = (rand() % crossesPerX) * (kStreetWidth + kBuildingSize) + (float)kStreetWidth * 0.5 + (float)dis(e);
	u.posY = (float)disPos(e);
	u.posZ = (rand() % crossesPerY) * (kStreetWidth + kBuildingSize) + (float)kStreetWidth * 0.5 + (float)dis(e);

	u.power = (float)dis(e);
	u.prevDirIdx = 0;
	u.prevCrossIdx = 0;

	PendingUnit pendingUnit;
	pendingUnit.uuid = uuid;
	pendingUnit.unit = u;
	m_unitsToAdd.push_back(pendingUnit);

	return kAddOk;
}


const Unit* Units::GetUnit(const UUID& uuid)
{
	std::lock_guard<std::mutex> lck (m_mutex);

	if(m_uuidToIdx.find(uuid) == m_uuidToIdx.end())
		return nullptr;

	uint32_t idx = m_uuidToIdx[uuid];
	return &m_units[idx];
}


uint32_t Units::GetUnitsNumber() const
{
	return m_units.size();
}


uint32_t Units::AddPendingUnits()
{
	std::lock_guard<std::mutex> lck(m_mutex);
	
	uint32_t ret = m_unitsToAdd.size();
	for (auto& u : m_unitsToAdd)
	{
		u.unit.index = m_units.size();
		m_uuidToIdx[u.uuid] = u.unit.index;
		m_units.push_back(u.unit);
		
		size_t recordSize = u.uuid.size() + sizeof(Unit);
		fseek(m_storage, recordSize * u.unit.index, SEEK_SET);
		fwrite(u.uuid.data(), u.uuid.size(), 1, m_storage);
		fwrite(&u.unit, sizeof(Unit), 1, m_storage);
	}
	m_unitsToAdd.clear();
	fflush(m_storage);
	
	if(ret)
		printf("Number of units: %u %u\n", m_units.size(), m_uuidToIdx.size());

	return ret;
}


void Units::Simulate(const Texture2D& randomTex)
{
	if (!m_simulationProgram)
		return;

	{
		if (!m_issuedCopyBuffers.empty())
		{
			CopyBuffer& buf = m_issuedCopyBuffers.front();
			GLenum ret = glClientWaitSync(buf.sync, 0, 0);
			if (ret == GL_WAIT_FAILED)
			{
				CheckError("glClientWaitSync");
			}
			else if (ret == GL_ALREADY_SIGNALED || ret == GL_CONDITION_SATISFIED)
			{
				glBindBuffer(GL_COPY_WRITE_BUFFER, buf.buffer);
				glGetBufferSubData(GL_COPY_WRITE_BUFFER, 0, sizeof(Unit) * buf.unitsCopied, m_units.data());
				glBindBuffer(GL_COPY_WRITE_BUFFER, 0);
			}

			if (ret != GL_TIMEOUT_EXPIRED)
			{
				glDeleteSync(buf.sync);
				buf.sync = 0;
				buf.unitsCopied = 0;
				m_freeCopyBuffers.push_back(buf);
				m_issuedCopyBuffers.pop_front();
			}
		}

		uint32_t unitsAdded = AddPendingUnits();
		if (unitsAdded)
		{
			glBindBuffer(GL_SHADER_STORAGE_BUFFER, m_ssbo);
			uint32_t offset = m_units.size() - unitsAdded;
			void* ptr = (void*)(m_units.data() + offset);
			glBufferSubData(GL_SHADER_STORAGE_BUFFER, offset * sizeof(Unit), unitsAdded * sizeof(Unit), ptr);
			glBindBuffer(GL_SHADER_STORAGE_BUFFER, 0);
		}
	}

	if (m_units.size())
	{
		glUseProgram(m_simulationProgram->GetProgram());
		m_simulationProgram->SetIVec4("unitsCount", glm::ivec4(m_units.size(), 0, 0, 0));
		m_simulationProgram->SetVec4("fieldSize", glm::vec4((float)m_fieldSizeX, (float)m_fieldSizeY, 0, 0));
		m_simulationProgram->SetTexture("randomTex", randomTex);
		m_simulationProgram->SetSSBO("Units", m_ssbo);
		m_simulationProgram->BindUniforms();
		glDispatchCompute((m_units.size() + 31) / 32, 1, 1);

		glMemoryBarrier(GL_SHADER_STORAGE_BARRIER_BIT);
		
		CopyBuffer copyBuf;
		if (m_freeCopyBuffers.empty())
		{
			glGenBuffers(1, &copyBuf.buffer);
			glBindBuffer(GL_COPY_WRITE_BUFFER, copyBuf.buffer);
			glBufferData(GL_COPY_WRITE_BUFFER, kMaxUnitsCount * sizeof(Unit), nullptr, GL_DYNAMIC_DRAW);
			glBindBuffer(GL_COPY_WRITE_BUFFER, 0);
		}
		else
		{
			copyBuf = m_freeCopyBuffers.front();
			m_freeCopyBuffers.pop_front();
		}
		glCopyNamedBufferSubData(m_ssbo, copyBuf.buffer, 0, 0, m_units.size() * sizeof(Unit));
		copyBuf.unitsCopied = m_units.size();
		copyBuf.sync = glFenceSync(GL_SYNC_GPU_COMMANDS_COMPLETE, 0);
		m_issuedCopyBuffers.push_back(copyBuf);
	}
}


void Units::Draw(const glm::mat4& projMatrix, GLuint cameraDataSsbo)
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
	glBindBufferBase(GL_SHADER_STORAGE_BUFFER, 2, cameraDataSsbo);
	
	m_visibilityProgram->SetIVec4("unitsCount", glm::ivec4(m_units.size(), 0, 0, 0));
	m_visibilityProgram->BindUniforms();
	glDispatchCompute((m_units.size() + 31) / 32, 1, 1);
	glMemoryBarrier(GL_VERTEX_ATTRIB_ARRAY_BARRIER_BIT | GL_ATOMIC_COUNTER_BARRIER_BIT);

	glBindVertexArray(m_vao);

	glUseProgram(m_visualizationProgram->GetProgram());
	m_visualizationProgram->SetMat4("projMatrix", projMatrix);
	m_visualizationProgram->SetSSBO("Units", m_ssbo);
	m_visualizationProgram->SetSSBO("CameraData", cameraDataSsbo);
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


GLuint Units::GetSSBO() const
{
	return m_ssbo;
}
