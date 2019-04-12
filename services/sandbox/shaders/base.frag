#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(binding = 0) uniform sampler2D units;
//layout(binding = 1) uniform sampler2D map;
uniform vec4 targetSize;

layout(location = 0) out vec4 outColor;

void main()
{
	vec2 uv = gl_FragCoord.xy / targetSize.xy;
	uv.y = 1.0f - uv.y;
	outColor = texture(units, uv);// + texture(map, uv);
}
