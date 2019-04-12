#version 450
#extension GL_ARB_separate_shader_objects : enable

layout (location = 0) in vec3 vertex_pos;

uniform mat4 viewProjMatrix;

void main()
{
	gl_Position = viewProjMatrix * vec4(vertex_pos, 1.0f);
}
