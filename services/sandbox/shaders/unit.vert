#version 450
#extension GL_ARB_separate_shader_objects : enable

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
	uint type;
	float power;
	uint prevDirIdx;
	uint prevCrossIdx;
};

layout(std430, binding = 8) buffer Units
{
	Unit units[];
};

uniform mat4 viewProjMatrix;
uniform mat4 viewMatrix;

out vec4 color;

void main()
{
	Unit unit = units[gl_InstanceID];

	vec4 vertexPos = vec4(0.0f, 0.0f, 0.0, 1.0);
	vertexPos.xyz = vec3(positions[gl_VertexID].x) * viewMatrix[0].xyz + vec3(positions[gl_VertexID].y) * viewMatrix[1].xyz;
	vertexPos.xyz += vec3(unit.posX, unit.posY, unit.posZ);

	gl_Position = viewProjMatrix * vertexPos;

	color = vec4(0.0f);
	color.x = unit.mind[0] & 0xff;
	color.y = unit.mind[1] & 0xff;
	color.z = unit.mind[2] & 0xff;
	color.w = 255.0f;
	color /= vec4(255.0f);
}
