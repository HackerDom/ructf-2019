#pragma once
#include "glwrap.h"
#include <vector>

enum kUnitType
{
	EUnitHuman = 0,
	EUnitSmith
};


struct Unit
{
	uint32_t mind[8];
	uint32_t id;
	float posX;
	float posY;
	float posZ;
	uint32_t type;
	float power;
	uint32_t prevDirIdx;
	uint32_t prevCrossIdx;
};
static_assert (sizeof(Unit) == 16 * 4, "hey!");


class Units
{
public:

	Units() = default;
	~Units() = default;
	bool Init(uint32_t numUnits, uint32_t fieldSizeX, uint32_t fieldSizeY);
	void Shutdown();

	void Simulate(const Texture2D& target, const Texture2D& randomTex);
	void Draw(const glm::mat4& viewProjMatrix, const glm::mat4& viewMatrix);

private:
	GLuint m_ssbo = 0;

	VertexShader* m_visualizationVs = nullptr;
	FragmentShader* m_visualizationFs = nullptr;
	Program* m_visualizationProgram = nullptr;
	GLuint m_dummyVao = 0;

	ComputeShader* m_simulationCs = nullptr;
	Program* m_simulationProgram = nullptr;

	std::vector<Unit> m_units;

	void GenerateUnits(uint32_t numUnits, uint32_t fieldSizeX, uint32_t fieldSizeY);
};