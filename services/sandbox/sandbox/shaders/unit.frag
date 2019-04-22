#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(location = 0) out vec4 outColor;

in vec2 pos2d;

void main()
{
	float dist = 0.5f - length(pos2d);
	if(dist < 0.1f)
		discard;
	outColor = vec4(247.0f, 236.0f, 68.0f, 255.0f) / 255.0f * sqrt(dist);
}
