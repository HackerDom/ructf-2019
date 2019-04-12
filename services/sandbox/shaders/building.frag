#version 450
#extension GL_ARB_separate_shader_objects : enable

uniform vec4 targetSize;

layout(location = 0) out vec4 outColor;

void main()
{
	outColor = vec4(0.0f, 1.0f, 0.0f, 1.0f);
}
