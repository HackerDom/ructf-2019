#include "EGL/egl.h"

enum Format
{
	FORMAT_A8,
	FORMAT_RGBA,
	FORMAT_COUNT
};

bool Init();
void Shutdown();
