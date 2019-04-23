#version 450
#extension GL_ARB_separate_shader_objects : enable

in vec2 uv;

layout(location = 0) out vec4 outColor;
layout(binding = 0) uniform sampler2D tex;

void main()
{
	outColor = texture(tex, uv) * vec4(207.0f, 2.0f, 159.0f, 255.0f) / 255.0f;
}
