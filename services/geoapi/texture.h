#pragma once
#include <stdio.h>
#include "egl.h"
#include <string.h>
#include "GLES2/gl2.h"
#include <cstdlib>

enum Format
{
	FORMAT_A8,
	FORMAT_RGBA,
	FORMAT_COUNT
};

class Texture2D
{
public:
	Texture2D() = delete;
	Texture2D( int width, int height, Format format, void* initData = nullptr );
	~Texture2D();

	GLuint GetTexture() const;
	GLuint GetFramebuffer() const;

	void ReadBack();

private:
	GLuint 	m_texture = 0;
	GLuint 	m_framebuffer = 0;
	int 	m_width = 0;
	int		m_height = 0;
	Format 	m_format = FORMAT_COUNT;
	GLubyte*	m_shadowCopy = nullptr;

    bool Init( int width, int height, Format format, void* initData );
};
