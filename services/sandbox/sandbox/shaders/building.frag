#version 450
#extension GL_ARB_separate_shader_objects : enable

in vec3 barycentricCoords;
in vec3 normal;

layout(location = 0) out vec4 outColor;

uniform vec4 viewDir;

void main()
{
	float dotNV = dot(normal, -viewDir.xyz);
	dotNV = clamp(dotNV, 0.0f, 1.0f);
	outColor = vec4(0.0f, 0.0f, 0.0f, 0.0f);
	
	const float edgeThikness = 1.2f;
	vec3 fw = abs(dFdx(barycentricCoords)) + abs(dFdy(barycentricCoords));
	vec3 val = smoothstep(vec3(0.0f, 0.0f, 0.0f), fw * edgeThikness, barycentricCoords);
	float edge = min(min(val.x, val.y), val.z);
	if(edge <= 0.5f)
		outColor.rgb = vec3(207.0f, 2.0f, 159.0f) / 255.0f;
}
