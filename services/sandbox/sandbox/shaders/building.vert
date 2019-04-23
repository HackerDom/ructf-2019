#version 450
#extension GL_ARB_separate_shader_objects : enable

layout (location = 0) in vec3 vertexPos;
layout (location = 1) in vec2 vertexUv;
layout (location = 2) in uint instanceId;

uniform mat4 projMatrix;
uniform ivec4 numBuildings;
uniform vec4 buildingSize;

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

out vec2 uv;

void main()
{
	int xiOffset = int(instanceId) % numBuildings.x;
	int ziOffset = int(instanceId) / numBuildings.x;

	float xOffset = float(xiOffset) * (buildingSize.x + buildingSize.y) + buildingSize.x * 0.5f + buildingSize.y;
	float zOffset = float(ziOffset) * (buildingSize.x + buildingSize.y) + buildingSize.x * 0.5f + buildingSize.y;

	vec4 worldPos = vec4(vertexPos * buildingSize.x, 1.0f);
	worldPos.xz += vec2(xOffset, zOffset);

	gl_Position = projMatrix * ViewMatrix * worldPos;
	uv = vertexUv;
}
