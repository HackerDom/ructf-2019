#include "glwrap.h"

bool CheckError( const char* errorMsgPrefix )
{
	GLenum err = glGetError();
	if( err != GL_NO_ERROR ) {
		fprintf( stderr, "%s: %x\n", errorMsgPrefix, err );
		return false;
	}

	return true;
}
