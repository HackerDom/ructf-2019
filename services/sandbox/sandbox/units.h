#pragma once
#include "glwrap.h"
#include <vector>
#include <map>
#include <array>
#include "../uuid.h"


enum EUnitType
{
	kUnitHuman = 0,
	kUnitSmith
};


struct Unit
{
	uint32_t mind[8];
	uint32_t index;
	float posX;
	float posY;
	float posZ;
	uint32_t type;
	float power;
	uint32_t prevDirIdx;
	uint32_t prevCrossIdx;
};
static_assert (sizeof(Unit) == 16 * 4, "hey!");


static const uint32_t kMaxUnitsCount = 512 * 1024;


class Units
{
public:

	enum EAddResult
	{
		kAddOk = 0,
		kAddTooMuchUnits,
		kAddAlreadyExists
	};


	Units() = default;
	~Units() = default;
	bool Init(uint32_t fieldSizeX, uint32_t fieldSizeY);
	void Shutdown();

	EAddResult AddUnit(const UUID& uuid, uint32_t mind[8]);
	const Unit* GetUnit(const UUID& uuid);

	void Simulate(const Texture2D& target, const Texture2D& randomTex);
	void Draw(const glm::mat4& viewProjMatrix, const glm::mat4& viewMatrix, const glm::vec4 frustumPlanes[]);

private:
	GLuint m_ssbo = 0;

	VertexShader* m_visualizationVs = nullptr;
	FragmentShader* m_visualizationFs = nullptr;
	Program* m_visualizationProgram = nullptr;
	GLuint m_vao = 0;
	GLuint m_instancesVbo = 0;

	ComputeShader* m_visibilityCs = nullptr;
	Program* m_visibilityProgram = nullptr;
	GLuint m_indirectBuffer = 0;

	ComputeShader* m_simulationCs = nullptr;
	Program* m_simulationProgram = nullptr;

	uint32_t m_fieldSizeX;
	uint32_t m_fieldSizeY;
	std::vector<Unit> m_units;
	std::map<UUID, uint32_t> m_uuidToIdx;

	struct PendingUnit
	{
		UUID uuid;
		Unit unit;
	};
	std::vector<PendingUnit> m_unitsToAdd;

	void AddPendingUnits();
};
