#pragma once
#include <stdint.h>
#include "../commands.h"
#include "../uuid.h"


struct UnitDesc
{
    uint32_t mind[8];
	float posX;
	float posY;
	float posZ;
	float power;
};

extern EAddUnitResult AddUnit(const char* mind, char* uuid);
extern bool GetUnit(const char* uuid, UnitDesc& desc, bool& found);