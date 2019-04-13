#pragma once
#include <GL/glew.h>
#include <GLFW/glfw3.h>
#include <glm/vec4.hpp>
#include <glm/mat4x4.hpp>
#include <stdint.h>
#include <string.h>
#include "gl.h"
#include "texture.h"
#include "shader.h"
#include "png.h"


struct DrawElementsIndirectCommand 
{
	uint32_t count;
	uint32_t primCount;
	uint32_t firstIndex;
	uint32_t baseVertex;
	uint32_t baseInstance;
};