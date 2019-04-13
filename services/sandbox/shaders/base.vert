#version 450
#extension GL_ARB_separate_shader_objects : enable

vec2 kPositions[6] = vec2[](
    vec2(-1.0f, -1.0f),
    vec2( 1.0f, -1.0f),
    vec2( 1.0f,  1.0f),
    vec2(-1.0f, -1.0f),
    vec2( 1.0f,  1.0f),
    vec2(-1.0f,  1.0f)
);


vec2 kUVs[6] = vec2[](
	vec2( 0.0f,  1.0f),
    vec2( 1.0f,  1.0f),
    vec2( 1.0f,  0.0f),
    vec2( 0.0f,  1.0f),
    vec2( 1.0f,  0.0f),
    vec2( 0.0f,  0.0f)
);

out vec2 uv;

void main()
{
	gl_Position = vec4(kPositions[gl_VertexID] * 0.3, 0.0, 1.0);
	uv = kUVs[gl_VertexID];
}
