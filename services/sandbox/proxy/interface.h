#pragma once
#include <stdint.h>

struct UnitDesc
{
    uint32_t mind[8];
	float posX;
	float posY;
	float posZ;
	float power;
};

extern bool AddUnit(const char* mind, float power, uint32_t& id);
extern bool GetUnit(uint32_t id, UnitDesc& desc, bool& found);