#version 450
#extension GL_ARB_separate_shader_objects : enable

layout (location = 0) in uint instanceId;

vec2 positions[6] = vec2[](
    vec2(-0.5f, -0.5f),
    vec2( 0.5f, -0.5f),
    vec2( 0.5f,  0.5f),
    vec2(-0.5f, -0.5f),
    vec2( 0.5f,  0.5f),
    vec2(-0.5f,  0.5f)
);

struct Unit
{
	uint mind[8];
	uint id;
	float posX;
	float posY;
	float posZ;
	uint padding;
	float power;
	uint prevDirIdx;
	uint prevCrossIdx;
};

layout(std430, binding = 0) buffer CameraData
{
    mat4 ViewMatrix;
    vec4 CameraPos;
    vec4 CameraDir;
    vec4 CameraUp;
    vec4 FrustumPlanes[6];
    uint forceMode;
    uint padding[3];
};

layout(std430, binding = 8) buffer Units
{
	Unit units[];
};

uniform mat4 projMatrix;

out vec2 pos2d;
out vec3 posWS;

void main()
{
	Unit unit = units[instanceId];

	mat4 trViewMatrix = transpose(ViewMatrix);

	vec4 vertexPos = vec4(0.0f, 0.0f, 0.0, 1.0);
	vertexPos.xyz = vec3(positions[gl_VertexID].x) * trViewMatrix[0].xyz + vec3(positions[gl_VertexID].y) * trViewMatrix[1].xyz;
	vertexPos.xyz += vec3(unit.posX, unit.posY, unit.posZ);
	posWS = vertexPos.xyz;

	gl_Position = projMatrix * ViewMatrix * vertexPos;
	pos2d = positions[gl_VertexID];
}
