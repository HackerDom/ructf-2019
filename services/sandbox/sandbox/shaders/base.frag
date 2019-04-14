#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(binding = 0) uniform usampler2D units;
//layout(binding = 1) uniform sampler2D map;
uniform vec4 targetSize;

layout(location = 0) out vec4 outColor;

in vec2 uv;

void main()
{
	outColor = texture(units, uv);// + texture(map, uv);
}
