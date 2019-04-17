#pragma once
#include <stdint.h>
#include "../uuid.h"


struct UnitDesc
{
    uint32_t mind[8];
	float posX;
	float posY;
	float posZ;
	float power;
};

extern bool AddUnit(const char* mind, float power, char* uuid);
extern bool GetUnit(const char* uuid, UnitDesc& desc, bool& found);