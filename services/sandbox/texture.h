#pragma once
#include "png.h"

enum Format
{
	FORMAT_A8,
	FORMAT_RGBA8,
	FORMAT_RGBA32,
	FORMAT_COUNT
};


class Texture2D
{
public:
	Texture2D() = delete;
	Texture2D(int width, int height, Format format, void* initData = nullptr);
	Texture2D(const void* png, uint32_t size);
	Texture2D(const Image& image);
	~Texture2D();

	GLuint GetTexture() const;
	//GLuint GetFramebuffer() const;
	int GetWidth() const;
	int GetHeight() const;
	Format GetFormat() const;
	GLint GetGlFormat() const;

private:
	GLuint m_texture = 0;
	//GLuint m_framebuffer = 0;
	int m_width = 0;
	int m_height = 0;
	Format m_format = FORMAT_COUNT;

	bool Init(int width, int height, Format format, void* initData = nullptr);
};
