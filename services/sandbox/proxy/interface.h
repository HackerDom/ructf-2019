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

enum EAddUnitResult
{
	kAddUnitOk = CommandAddUnitResponse::kOk,
	kAddUnitTooMuchUnits = CommandAddUnitResponse::kTooMuchUnits,
	kAddUnitAlreadyExists = CommandAddUnitResponse::kAlreadyExists,
	kAddUnitInternalError = CommandAddUnitResponse::kInternalError,
	kAddUnitBadUUID
};

typedef EAddUnitResult TAddUnit(const char*, const char*);
typedef bool TGetUnit(const char*, UnitDesc&, bool&);