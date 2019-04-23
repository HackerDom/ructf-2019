#version 450
#extension GL_ARB_separate_shader_objects : enable

in vec2 uv;

layout(location = 0) out vec4 outColor;
layout(binding = 0) uniform sampler2D tex;

void main()
{
	outColor = texture(tex, uv);
}
