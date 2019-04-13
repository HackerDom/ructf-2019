#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include "png.h"

#if HAS_LIBPNG
#include <png.h>

void PNGAPI error_function(png_structp png, png_const_charp dummy)
{
	(void)dummy;
	longjmp(png_jmpbuf(png), 1);
}


bool read_png(const char* file_name, Image& image)
{
	png_structp png = nullptr;
	FILE* fp = nullptr;
	auto errorHandler = [&]() {
		png_destroy_read_struct(&png, NULL, NULL);
		fclose(fp);
	};

	fp = fopen(file_name, "rb");
	if (fp == NULL)
		return false;

	png = png_create_read_struct(PNG_LIBPNG_VER_STRING, 0, 0, 0);
	if (png == NULL)
	{
		errorHandler();
		return false;
	}

	png_set_error_fn(png, 0, error_function, NULL);
	if (setjmp(png_jmpbuf(png)))
	{
		errorHandler();
		return false;
	}

	png_infop info = png_create_info_struct(png);
	if (info == nullptr)
	{
		errorHandler();
		return false;
	}

	png_init_io(png, fp);
	png_read_info(png, info);

	int color_type, bit_depth;
	png_uint_32 width, height;
	if (!png_get_IHDR(png, info, &width, &height, &bit_depth, &color_type, nullptr, nullptr, nullptr))
	{
		errorHandler();
		return false;
	}
	image.width = width;
	image.height = height;

	if (color_type == PNG_COLOR_TYPE_PALETTE && bit_depth <= 8)
		png_set_expand(png);

	if (color_type == PNG_COLOR_TYPE_GRAY && bit_depth < 8)
		png_set_expand(png);

	if (png_get_valid(png, info, PNG_INFO_tRNS))
		png_set_expand(png);

	if (bit_depth == 16)
		png_set_strip_16(png);

	if (bit_depth < 8)
		png_set_packing(png);

	if (color_type == PNG_COLOR_TYPE_RGB)
		png_set_filler(png, 255, PNG_FILLER_AFTER);

	png_read_update_info(png, info);
	image.rgba = new RGBA[image.width * image.height];
	RGBA* rows[height];
	RGBA* p = image.rgba;
	for (uint32_t i = 0; i < image.height; i++)
	{
		rows[i] = p;
		p += image.width;
	}
	png_read_image(png, (png_bytepp)rows);

	png_read_end(png, info);
	png_destroy_read_struct(&png, &info, NULL);

	fclose(fp);
	return true;
}


bool save_png(const char* file_name, const Image& image)
{
	save_png(file_name, image.rgba, image.width, image.height);
}


bool save_png(const char* file_name, const RGBA* rgba, uint32_t width, uint32_t height)
{
	png_structp png;
	png_infop info;
	png_uint_32 y;

	png = png_create_write_struct(PNG_LIBPNG_VER_STRING, NULL, NULL, NULL);
	if (png == NULL)
		return false;

	info = png_create_info_struct(png);
	if (info == NULL)
	{
		png_destroy_write_struct(&png, NULL);
		return false;
	}

	if (setjmp(png_jmpbuf(png)))
	{
		png_destroy_write_struct(&png, &info);
		return false;
	}
	FILE* fp = NULL;

	fp = fopen(file_name, "wb");
	if (fp == NULL)
	{
		png_destroy_write_struct(&png, &info);
		return false;
	}
	png_init_io(png, fp);
	png_set_IHDR(png, info, width, height, 8, PNG_COLOR_TYPE_RGB_ALPHA, PNG_INTERLACE_NONE,
	             PNG_COMPRESSION_TYPE_DEFAULT, PNG_FILTER_TYPE_DEFAULT);
	png_write_info(png, info);
	for (y = 0; y < height; ++y)
	{
		png_bytep row = (png_bytep)(rgba + y * width);
		png_write_rows(png, &row, 1);
	}
	png_write_end(png, info);
	png_destroy_write_struct(&png, &info);
	fclose(fp);
	return true;
}
#endif