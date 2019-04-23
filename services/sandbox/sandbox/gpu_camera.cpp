#include "gpu_camera.h"


bool GpuCamera::Init(const glm::mat4& viewMatrix, const glm::vec3& cameraPos, const glm::vec3& cameraDir, const glm::vec3& cameraUp)
{
	m_cs = new ComputeShader("shaders/gpu_camera.cs");
	if (!m_cs->IsValid())
		return false;
	Shader* shaders[] = {m_cs};
	m_program = new Program(shaders, 1);
	if (!m_program->IsValid())
		return false;

	glGenBuffers(1, &m_dataSsbo);
	glBindBuffer(GL_SHADER_STORAGE_BUFFER, m_dataSsbo);
	m_data.viewMatrix = viewMatrix;
	m_data.cameraPos = cameraPos;
	m_data.cameraDir = cameraDir;
	m_data.cameraUp = cameraUp;
	m_data.forceMode = 0;
	glBufferData(GL_SHADER_STORAGE_BUFFER, sizeof(Data), &m_data, GL_DYNAMIC_DRAW);
	glBindBuffer(GL_SHADER_STORAGE_BUFFER, 0);

	return true;
}


void GpuCamera::Shutdown()
{
	if (m_program)
	{
		delete m_program;
		m_program = nullptr;
	}
	if (m_cs)
	{
		delete m_cs;
		m_cs = nullptr;
	}

	DeleteBuffer(m_dataSsbo);
}


void GpuCamera::EnableForceMode(bool enable)
{
	m_data.forceMode = enable ? 1 : 0;
	m_dataIsDirty = true;
}


bool GpuCamera::IsForceModeEnabled() const
{
	return m_data.forceMode ? true : false;
}


void GpuCamera::ForceCameraData(const glm::vec3& cameraPos, const glm::vec3& cameraDir, const glm::vec3& cameraUp)
{
	m_data.cameraPos = cameraPos;
	m_data.cameraDir = cameraDir;
	m_data.cameraUp = cameraUp;
	m_dataIsDirty = true;
}


void GpuCamera::SetUnitToFollow(int idx)
{
	m_unitToFollow = idx;
}


int GpuCamera::GetUnitIdxToFollow() const
{
	return m_unitToFollow;
}


void GpuCamera::Update(float fovy, float aspect, float znear, float zfar, GLuint unitsSsbo)
{
	if (m_dataIsDirty)
	{
		glBindBuffer(GL_SHADER_STORAGE_BUFFER, m_dataSsbo);
		glBufferSubData(GL_SHADER_STORAGE_BUFFER, 0, sizeof(Data), &m_data);
		glBindBuffer(GL_SHADER_STORAGE_BUFFER, 0);
		m_dataIsDirty = false;
	}

	glUseProgram(m_program->GetProgram());
	m_program->SetSSBO("CameraData", m_dataSsbo);
	m_program->SetSSBO("Units", unitsSsbo);
	m_program->SetVec4("ProjData", glm::vec4(fovy, aspect, znear, zfar));
	m_program->SetIVec4("UnitIdxToFollow", glm::ivec4(m_unitToFollow, 0, 0, 0));
	m_program->BindUniforms();
	glDispatchCompute(1, 1, 1);

	glMemoryBarrier(GL_SHADER_STORAGE_BARRIER_BIT);
}


GLuint GpuCamera::GetDataSSBO()
{
	return m_dataSsbo;
}