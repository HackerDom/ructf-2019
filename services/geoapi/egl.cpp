#include <GLES2/gl2.h>
#include "egl.h"
#include <stdio.h>

#define ENABLE_RUNTIME_DEBUG 0

static const EGLint configAttribs[] = {
    EGL_SURFACE_TYPE, EGL_PBUFFER_BIT,
    EGL_BLUE_SIZE, 8,
    EGL_GREEN_SIZE, 8,
    EGL_RED_SIZE, 8,
    EGL_DEPTH_SIZE, 8,
    EGL_RENDERABLE_TYPE, EGL_OPENGL_ES2_BIT,
    EGL_NONE
};


static const EGLint pbufferAttribs[] = {
    EGL_WIDTH, 8,
    EGL_HEIGHT, 8,
    EGL_NONE,
};


static const EGLint context_attribute_list[] = {
	EGL_CONTEXT_CLIENT_VERSION, 2,
	EGL_NONE
};


Context g_context;


bool InitEGL()
{
    g_context.display = eglGetDisplay( EGL_DEFAULT_DISPLAY );
    if( g_context.display == EGL_NO_DISPLAY ) {
        printf( "Error: No display found! 0x%X\n", eglGetError() );
        return false;
    }

    EGLint major, minor;
    if( !eglInitialize( g_context.display, &major, &minor ) ) {
        printf( "eglInitialize failed 0x%X\n", eglGetError() );
        return false;
    }

    EGLint numConfigs;
    EGLConfig eglCfg;
    eglChooseConfig( g_context.display, configAttribs, &eglCfg, 1, &numConfigs );

    g_context.surface = eglCreatePbufferSurface( g_context.display, eglCfg, pbufferAttribs );
    if( g_context.surface == EGL_NO_SURFACE ){
        printf( "eglCreatePbufferSurface failed 0x%X\n", eglGetError() );
        return false;
    }

    g_context.context = eglCreateContext( g_context.display, eglCfg, EGL_NO_CONTEXT, context_attribute_list );
    if( g_context.context == EGL_NO_CONTEXT ){
        printf( "eglCreateContext failed 0x%X\n", eglGetError() );
        return false;
    }

    eglMakeCurrent( g_context.display, g_context.surface, g_context.surface, g_context.context );

    return true;
}


void ShutdownEGL()
{
    eglTerminate( g_context.display );
}

bool CheckError( const char* errorMsgPrefix )
{
	GLenum err = glGetError();
	if( err != GL_NO_ERROR ) {
		fprintf( stderr, "%s: %x\n", errorMsgPrefix, err );
		return false;
	}

	return true;
}
