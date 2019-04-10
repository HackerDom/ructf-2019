#pragma once
#include <stdint.h>


//
union RGBA
{
	struct
	{
		uint32_t r : 8;
		uint32_t g : 8;
		uint32_t b : 8;
		uint32_t a : 8;
	};
	uint32_t rgba;
};


//
struct Image
{
	RGBA* rgba;
	uint32_t width;
	uint32_t height;

	Image() : rgba(nullptr), width(0), height(0)
	{
	}

	Image(uint16_t w, uint16_t h) : rgba(nullptr), width(w), height(h)
	{
		rgba = new RGBA[w * h];
	}

	Image(const Image&) = delete;
	Image(const Image&&) = delete;
	Image& operator=(const Image&) = delete;

	~Image()
	{
		delete[] rgba;
	}

	//
	void Reinit(uint16_t w, uint16_t h)
	{
		delete[] rgba;
		width = w;
		height = h;
		rgba = new RGBA[w * h];
	}
};


#if HAS_LIBPNG
bool read_png(const char* file_name, Image& image);
bool save_png(const char* file_name, const Image& image);
bool save_png(const char* file_name, const RGBA* rgba, uint32_t width, uint32_t height);
#endif