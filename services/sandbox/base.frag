#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(binding = 0) uniform sampler2D img_input;
uniform vec4 targetSize;

layout(location = 0) out vec4 outColor;

void main()
{
	vec2 uv = gl_FragCoord.xy / targetSize.xy;
	outColor = texture(img_input, uv);
}
