#pragma once

enum ECommand : uint32_t
{
	kCommandAddUnit = 0,
	kCommandGetUnit,

	kCommandsCount
};


struct CommandHeader
{
	ECommand cmd;
};


struct CommandAddUnit
{
	uint32_t mind[8];
	float power;
};


struct CommandAddUnitResponse
{
	uint8_t uuid[16];
};


struct CommandGetUnit
{
	uint8_t uuid[16];
};


struct CommandGetUnitResponse
{
	bool ok;
	uint32_t mind[8];
	float posX;
	float posY;
	float posZ;
	float power;
};
