#pragma once
#include "glwrap.h"

static const uint32_t kBuildingSize = 24;
static const uint32_t kStreetWidth = 8;

class Buildings
{
public:

	Buildings() = default;
	~Buildings() = default;
	bool Init(uint32_t fieldSizeX, uint32_t fieldSizeY);
	void Shutdown();

	void Draw(const glm::mat4& viewProjMatrix, const glm::vec3& viewDir);

private:
	VertexShader* m_vs = nullptr;
	GeometryShader* m_gs = nullptr;
	FragmentShader* m_fs = nullptr;
	Program* m_program = nullptr;
	GLuint m_vao = 0;
	GLuint m_vbo = 0;
	GLuint m_ebo = 0;
	uint32_t m_indicesNum = 0;

	uint32_t m_numBuildingsX;
	uint32_t m_numBuildingsY;
};