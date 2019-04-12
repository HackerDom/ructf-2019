#pragma once
#include "glwrap.h"
#include <vector>

static const uint32_t kBuildingSize = 24;
static const uint32_t kStreetWidth = 8;

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

	void Draw(const glm::mat4& viewProjMatrix, const glm::vec3& viewDir);

	const Texture2D& GetMap() const;

private:
	VertexShader* m_vs = nullptr;
	GeometryShader* m_gs = nullptr;
	FragmentShader* m_fs = nullptr;
	Program* m_program = nullptr;
	GLuint m_vao = 0;
	GLuint m_vbo = 0;
	GLuint m_ebo = 0;
	uint32_t m_indicesNum = 0;

	uint32_t m_fieldSizeX;
	uint32_t m_fieldSizeY;
	uint32_t m_numBuildingsX;
	uint32_t m_numBuildingsY;

	std::vector<Building> m_buildings;
	Texture2D* m_map = nullptr;

	void GenerateBuildings();
};


inline const Texture2D& Buildings::GetMap() const
{
	return *m_map;
}