#pragma once

enum ECommand : uint32_t
{
	kCommandAddUnit = 0,

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
	uint32_t id;
};
