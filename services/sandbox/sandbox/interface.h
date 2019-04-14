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

typedef void (* TInterfaceCallback)(ECommand cmd, char* data, char*& response, uint32_t responseSize);

bool InitInterface(TInterfaceCallback callback);
void ShutdownInterface();
bool UpdateInterface();
