#include <stdio.h>
#include <png.h>
#include "glwrap.h"


struct TextureFormat
{
	GLint internalFormat;
	GLint format;
	GLint type;
} g_mapFormatToTextureFormat[] = {
    {GL_ALPHA, GL_ALPHA, GL_UNSIGNED_BYTE},
    {GL_RGBA, GL_RGBA, GL_UNSIGNED_BYTE},
	{GL_RGBA32F, GL_RGBA, GL_FLOAT},
	{GL_RGBA16F, GL_RGBA, GL_HALF_FLOAT},
};


void PNGAPI error_function(png_structp png, png_const_charp dummy);


struct ReadStruct
{
	const uint8_t* pngData = nullptr;
	uint32_t offset = 0;
};


void PngReadFn(png_structp png_ptr, png_bytep outBytes, png_size_t byteCountToRead)
{
	png_voidp io_ptr = png_get_io_ptr(png_ptr);
	if (!io_ptr)
		return;

	ReadStruct* rs = (ReadStruct*)io_ptr;
	memcpy(outBytes, rs->pngData + rs->offset, byteCountToRead);
	rs->offset += byteCountToRead;
}


Texture2D::Texture2D(int width, int height, Format format, void* initData)
	: m_width(0), m_height(0), m_format(FORMAT_COUNT)
{
	Init(width, height, format, initData);
}


Texture2D::Texture2D(const void* pngData, uint32_t size)
	: m_width(0), m_height(0), m_format(FORMAT_COUNT)
{
	png_structp png = nullptr;
	auto errorHandler = [&]() { png_destroy_read_struct(&png, NULL, NULL); };

	png = png_create_read_struct(PNG_LIBPNG_VER_STRING, 0, 0, 0);
	if (png == NULL)
	{
		errorHandler();
		return;
	}

	png_set_error_fn(png, 0, error_function, NULL);
	if (setjmp(png_jmpbuf(png)))
	{
		errorHandler();
		return;
	}

	ReadStruct rs;
	rs.pngData = (const uint8_t*)pngData;
	png_set_read_fn(png, &rs, PngReadFn);

	png_infop info = png_create_info_struct(png);
	if (info == nullptr)
	{
		errorHandler();
		return;
	}

	png_read_info(png, info);

	int color_type, bit_depth;
	png_uint_32 width, height;
	if (!png_get_IHDR(png, info, &width, &height, &bit_depth, &color_type, nullptr, nullptr, nullptr))
	{
		errorHandler();
		return;
	}

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

	if (!Init(width, height, FORMAT_RGBA8, nullptr))
	{
		errorHandler();
		return;
	}

	RGBA* rows[height];
	RGBA* p = (RGBA*)malloc(width * height * sizeof(RGBA));
	for (uint32_t i = 0; i < height; i++)
	{
		rows[i] = p;
		p += width;
	}

	png_read_image(png, (png_bytepp)rows);

	png_read_end(png, info);
	png_destroy_read_struct(&png, &info, NULL);

	TextureFormat fmt = g_mapFormatToTextureFormat[m_format];
	glTexImage2D(GL_TEXTURE_2D, 0, fmt.internalFormat, m_width, m_height, 0, fmt.format, fmt.type, p);
	free(p);
}


Texture2D::Texture2D(const Image& image) : Texture2D(image.width, image.height, FORMAT_RGBA8, (void*)image.rgba)
{
}


Texture2D::~Texture2D()
{
	glDeleteTextures(1, &m_texture);
	//glDeleteFramebuffers(1, &m_framebuffer);
	m_texture = 0;
	//m_framebuffer = 0;
}


bool Texture2D::Init(int width, int height, Format format, void* initData)
{
	glGenTextures(1, &m_texture);
	glBindTexture(GL_TEXTURE_2D, m_texture);

	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);

	TextureFormat fmt = g_mapFormatToTextureFormat[format];
	glTexImage2D(GL_TEXTURE_2D, 0, fmt.internalFormat, width, height, 0, fmt.format, fmt.type, initData);

	if (!CheckError("Failed to create texture"))
	{
		glDeleteTextures(1, &m_texture);
		m_texture = 0;
		return false;
	}

	/*glGenFramebuffers(1, &m_framebuffer);
	glBindFramebuffer(GL_FRAMEBUFFER, m_framebuffer);
	glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, m_texture, 0);
	glBindFramebuffer(GL_FRAMEBUFFER, 0);

	if (!CheckError("Failed to create framebuffer"))
	{
		glDeleteTextures(1, &m_texture);
		glDeleteFramebuffers(1, &m_framebuffer);
		m_texture = 0;
		m_framebuffer = 0;
		return false;
	}*/

	m_width = width;
	m_height = height;
	m_format = format;

	return true;
}


GLuint Texture2D::GetTexture() const
{
	return m_texture;
}


/*GLuint Texture2D::GetFramebuffer() const
{
	return m_framebuffer;
}*/


int Texture2D::GetWidth() const
{
	return m_width;
}


int Texture2D::GetHeight() const
{
	return m_height;
}


Format Texture2D::GetFormat() const
{
	return m_format;
}


GLint Texture2D::GetGlFormat() const
{
	return g_mapFormatToTextureFormat[m_format].internalFormat;
}
