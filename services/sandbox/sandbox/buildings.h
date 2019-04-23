#pragma once
#include "glwrap.h"
#include <vector>

static const uint32_t kBuildingSize = 24;
static const uint32_t kStreetWidth = 8;
static const uint32_t kLodsCount = 4;

struct Building
{
	float posX;
	float posY;
	float sizeX;
	float sizeY;
	float color[4];
};


class Buildings
{
public:

	Buildings() = default;
	~Buildings() = default;
	bool Init(uint32_t fieldSizeX, uint32_t fieldSizeY);
	void Shutdown();

	void Draw(const glm::mat4& projMatrix, GLuint cameraDataSsbo);

private:
	uint32_t m_lodIndicesNum[kLodsCount];

	VertexShader* m_vs = nullptr;
	GeometryShader* m_gs = nullptr;
	FragmentShader* m_fs = nullptr;
	Program* m_program = nullptr;
	struct Mesh
	{
		GLuint vao = 0;
		GLuint positionsVbo = 0;
		GLuint instancesVbo = 0;
		GLuint indicesBuffer = 0;
	};
	Mesh m_meshes[kLodsCount];

	uint32_t m_fieldSizeX;
	uint32_t m_fieldSizeY;
	uint32_t m_numBuildingsX;
	uint32_t m_numBuildingsY;

	ComputeShader* m_visCs = nullptr;
	Program* m_visProgram = nullptr;
	GLuint m_indirectBuffer[kLodsCount] = {};

	std::vector<Building> m_buildings;

	void GenerateBuildings();
};
