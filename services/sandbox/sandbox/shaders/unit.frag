#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(location = 0) out vec4 outColor;
layout(binding = 0) uniform sampler2D UnitTexture;

in vec2 pos2d;
in vec3 posWS;

void main()
{
	float angle = abs(posWS.x + posWS.z);
	angle *= 0.1f;
	mat2 rot = mat2(cos(angle), sin(angle), -sin(angle), cos(angle));

	vec2 uv = pos2d;
	uv = rot * uv;
	uv.y *= -1.0f;
	uv += vec2(0.5f, 0.5f);
	uv = clamp(uv, vec2(0.0f), vec2(1.0f));
	vec4 c = texture(UnitTexture, uv);
	if(c.a < 0.5)
		discard;
	outColor = c * vec4(247.0f, 236.0f, 68.0f, 255.0f) / 255.0f;
}
