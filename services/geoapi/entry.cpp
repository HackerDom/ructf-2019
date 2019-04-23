#include <stdio.h>
#include <stdlib.h>
#include "egl.h"
#include <string.h>
#include "GLES2/gl2.h"
//#include <GLFW/glfw3.h>
#include "texture.h"
#include "shader.h"

// sudo apt-get install libglfw3-dev libgles2-mesa-dev


const Texture2D* g_curFramebuffer = nullptr;

static const GLchar* vertex_shader_source =
    "#version 100\n"
    "attribute vec3 position;\n"

    "void main() {\n"
    "   gl_Position = vec4(position, 1.0);\n"
    "}\n";

static const GLchar* fragment_shader_source =
    "#version 100\n"
    "void main() {\n"
    "   	gl_FragColor = vec4(1.0, 1.0, 1.0, 1.0);"
    "}\n";

static const GLfloat vertices[] = {
	-1.0f,  -1.0f, 0.0f,
	1.0f,  -1.0f, 0.0f,
	1.0f,  1.0f, 0.0f,

	-1.0f,  -1.0f, 0.0f,
	1.0f,  1.0f, 0.0f,
	-1.0f,  1.0f, 0.0f,
};

GLint build_shader(const char *vertex_shader_source, const char *fragment_shader_source) {
    enum Consts {INFOLOG_LEN = 512};
    GLchar infoLog[INFOLOG_LEN];
    GLint fragment_shader;
    GLint shader_program;
    GLint success;
    GLint vertex_shader;

    /* Vertex shader */
    vertex_shader = glCreateShader(GL_VERTEX_SHADER);
    glShaderSource(vertex_shader, 1, &vertex_shader_source, NULL);
    glCompileShader(vertex_shader);
    glGetShaderiv(vertex_shader, GL_COMPILE_STATUS, &success);
    if (!success) {
        glGetShaderInfoLog(vertex_shader, INFOLOG_LEN, NULL, infoLog);
        printf("ERROR::SHADER::VERTEX::COMPILATION_FAILED\n%s\n", infoLog);
    }

    /* Fragment shader */
    fragment_shader = glCreateShader(GL_FRAGMENT_SHADER);
    glShaderSource(fragment_shader, 1, &fragment_shader_source, NULL);
    glCompileShader(fragment_shader);
    glGetShaderiv(fragment_shader, GL_COMPILE_STATUS, &success);
    if (!success) {
        glGetShaderInfoLog(fragment_shader, INFOLOG_LEN, NULL, infoLog);
        printf("ERROR::SHADER::FRAGMENT::COMPILATION_FAILED\n%s\n", infoLog);
    }

    shader_program = glCreateProgram();
    glAttachShader(shader_program, vertex_shader);
    glAttachShader(shader_program, fragment_shader);
    glLinkProgram(shader_program);
    glGetProgramiv(shader_program, GL_LINK_STATUS, &success);
    if (!success) {
        glGetProgramInfoLog(shader_program, INFOLOG_LEN, NULL, infoLog);
        printf("ERROR::SHADER::PROGRAM::LINKING_FAILED\n%s\n", infoLog);
    }

    glDeleteShader(vertex_shader);
    glDeleteShader(fragment_shader);
    return shader_program;
}

void SavePixels(int w, int h)
{
  GLubyte* pixels = new GLubyte[w*h*4];

  glPixelStorei( GL_PACK_ALIGNMENT, 1 );
  glReadPixels( 0, 0, w, h, GL_RGBA, GL_UNSIGNED_BYTE, pixels);
  CheckError( "glReadPixels failed" );

  size_t elmes_per_line = w * 4;
   for(size_t  i=0; i<w;  i++)
   {
     for(size_t  j=0; j<h;  j++)
     {
        size_t col = i * 4;
        size_t row = j * elmes_per_line;
        if (	pixels[row + col] == 255){
          printf("It's work\n");
          return;
        }
     }
     printf("\n");
   }
}

void BindFramebuffer( const Texture2D& t, GLuint width, GLuint height )
{
	glBindFramebuffer( GL_FRAMEBUFFER, t.GetFramebuffer() );
    glViewport( 0, 0, width, height );

  g_curFramebuffer = &t;
}

void Set4Vector(char* name, GLfloat* vector, GLuint program) {
  const int location = glGetUniformLocation( program, name );
  glUniform4fv( location, 1, 	vector);
}

void SetVector3Array(char* name, GLfloat* vectors, int size, GLuint program) {
  const int location = glGetUniformLocation( program, name );
  glUniform3fv(location, 2, vectors);
}

void Draw(GLuint width, GLuint height, GLuint shader_program ) {
  GLuint vbo;
  GLint pos, viewport;
  pos = glGetAttribLocation(shader_program, "position");

  GLfloat viewPortSize[] = {64.0, 64.0, 1.0, 1.0};

  GLfloat test[] = {
    width, height, 1.0,
    0.5, height, 1.0
  };

  //Set4Vector("viewPortSize", viewPortSize, shader_program);
  //SetVector3Array("uniformArrayOfStructs", test, 2, shader_program);

  glViewport(0, 0, width, height);

  glGenBuffers(1, &vbo);
  glBindBuffer(GL_ARRAY_BUFFER, vbo);
  glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);

	glVertexAttribPointer(pos, 3, GL_FLOAT, GL_FALSE, 0, (GLvoid*)0);
  glEnableVertexAttribArray(pos);

	//glVertexAttribPointer(viewport, 3, GL_FLOAT, GL_FALSE, 0, (GLvoid*)0);
  //glEnableVertexAttribArray(viewport);

  glBindBuffer(GL_ARRAY_BUFFER, 0);

  glClear(GL_COLOR_BUFFER_BIT);

  glUseProgram(shader_program);

  glDrawArrays(GL_TRIANGLES, 0, 6);

	glFinish();


  glDeleteBuffers(1, &vbo);
}

int main(void) {
		GLuint width = 64;
		GLuint height = 64;

    InitEGL();

	/*	GLFWwindow* window;

    glfwInit();
     glfwWindowHint(GLFW_CLIENT_API, GLFW_OPENGL_ES_API);
     glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 2);
     glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 0);
     window = glfwCreateWindow(width, height, __FILE__, NULL, NULL);

     glfwMakeContextCurrent(window);
*/
		Texture2D target( width, height, FORMAT_RGBA );

		VertexShader vs( "shaders/vertex.c", false );
    FragmentShader fs( "shaders/fragment.c", false );

    Program shader_program( vs, fs );

		BindFramebuffer( target, width, height );
    CheckError( "Bind error" );


		/*while (!glfwWindowShouldClose(window)) {
	         glfwPollEvents();
	         glClear(GL_COLOR_BUFFER_BIT);
					 Draw(width, height, shader_program.GetProgram());
           //SavePixels(width, height);
           //return 0;
	         glfwSwapBuffers(window);
	     }
*/
    glClear(GL_COLOR_BUFFER_BIT);
    Draw(width, height, shader_program.GetProgram());
    CheckError( "Draw error" );

    SavePixels(width, height);

    return EXIT_SUCCESS;
}
