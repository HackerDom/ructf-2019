#pragma once
#include "glwrap.h"


class GpuCamera
{
public:

	struct Data
	{
		glm::mat4 viewMatrix;
		glm::vec3 cameraPos;
		float padding0;
		glm::vec3 cameraDir;
		float padding1;
		glm::vec3 cameraUp;
		float padding2;
		glm::vec4 frustumPlanes[6];
		uint32_t forceMode;
		uint32_t padding3[3];
	};
	static_assert (sizeof(Data) == 224, "hey!");

	bool Init(const glm::mat4& viewMatrix, const glm::vec3& cameraPos, const glm::vec3& cameraDir, const glm::vec3& cameraUp);
	void Shutdown();

	void EnableForceMode(bool enable);
	bool IsForceModeEnabled() const;
	void ForceCameraData(const glm::vec3& cameraPos, const glm::vec3& cameraDir, const glm::vec3& cameraUp);
	void SetUnitToFollow(int idx);
	int GetUnitIdxToFollow() const;
	void Update(float fovy, float aspect, float znear, float zfar, GLuint unitsSsbo);
	GLuint GetDataSSBO();

private:

	GLuint m_dataSsbo = 0;

	ComputeShader* m_cs = nullptr;
	Program* m_program = nullptr;

	Data m_data;
	bool m_dataIsDirty = false;

	int32_t m_unitToFollow = -1;
};