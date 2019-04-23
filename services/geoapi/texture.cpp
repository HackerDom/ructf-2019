#include "texture.h"

struct TextureFormat
{
	GLint internalFormat;
	GLint format;
	GLint type;
} g_mapFormatToTextureFormat[] = {
	{ GL_ALPHA, GL_ALPHA, GL_UNSIGNED_BYTE },
	{ GL_RGBA, GL_RGBA, GL_UNSIGNED_BYTE },
};

//
struct ReadStruct
{
	const uint8_t* pngData = nullptr;
	uint32_t offset = 0;
};

//
Texture2D::Texture2D( int width, int height, Format format, void* initData )
    : m_width( 0 ), m_height( 0 ), m_format( FORMAT_COUNT ), m_shadowCopy( nullptr )
{
    Init( width, height, format, initData );
}

//
Texture2D::~Texture2D()
{
	glDeleteTextures( 1, &m_texture );
	glDeleteFramebuffers( 1, &m_framebuffer );
	m_texture = 0;
	m_framebuffer = 0;
    free( m_shadowCopy );
}

//
bool Texture2D::Init(int width, int height, Format format, void* initData)
{
	m_shadowCopy = new GLubyte[width * height*4];
    if( !m_shadowCopy ){
        glDeleteTextures( 1, &m_texture );
        m_texture = 0;
        return false;
    }

    if( initData )
        memcpy( m_shadowCopy, initData, width * height * sizeof( GLubyte ) * 4 );

    glGenTextures( 1, &m_texture );
    glBindTexture( GL_TEXTURE_2D, m_texture );

    glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST );
    glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST );


    TextureFormat fmt = g_mapFormatToTextureFormat[ format ];
    glTexImage2D( GL_TEXTURE_2D, 0, fmt.internalFormat, width, height, 0, fmt.format, fmt.type, m_shadowCopy );

    if( !CheckError( "Failed to create texture" ) ) {
        glDeleteTextures( 1, &m_texture );
        m_texture = 0;
        return false;
    }

    glGenFramebuffers( 1, &m_framebuffer );
    glBindFramebuffer( GL_FRAMEBUFFER, m_framebuffer );
    glFramebufferTexture2D( GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, m_texture, 0 );
    glBindFramebuffer( GL_FRAMEBUFFER, 0 );

    if( !CheckError( "Failed to create framebuffer" ) ) {
        glDeleteTextures( 1, &m_texture );
        glDeleteFramebuffers( 1, &m_framebuffer );
        m_texture = 0;
        m_framebuffer = 0;
        return false;
    }

    m_width = width;
    m_height = height;
    m_format = format;

    return true;
}

//
GLuint Texture2D::GetTexture() const
{
	return m_texture;
}

//
GLuint Texture2D::GetFramebuffer() const
{
	return m_framebuffer;
}

//
void Texture2D::ReadBack()
{
	glPixelStorei( GL_PACK_ALIGNMENT, 1 );
    glReadPixels( 0, 0, m_width, m_height, GL_RGBA, GL_UNSIGNED_BYTE, ( GLvoid* )m_shadowCopy );
    CheckError( "glReadPixels failed" );
}
