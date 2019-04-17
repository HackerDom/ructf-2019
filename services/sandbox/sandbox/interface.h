#pragma once
#include "../commands.h"

typedef void (* TInterfaceCallback)(const CommandHeader& cmd, char* data, char*& response, uint32_t& responseSize);

bool InitInterface(TInterfaceCallback callback);
void ShutdownInterface();
bool UpdateInterface();
