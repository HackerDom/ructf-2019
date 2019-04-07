#version 450

struct Unit
{
	uint mind[8];
	uint id;
	uint posX;
	uint posY;
	uint type;
	float power;
};

layout(std430, binding = 8) buffer Units
{
	Unit units[];
};

uniform ivec4 unitsCount;

layout(rgba32f, binding = 2) uniform image2D img_output;

uvec3 RandVector_v1(uvec3 p)
{
	uvec3 v = uvec3(p);
	v = v * 1664525u + 1013904223u;
	v.x += v.y*v.z;
	v.y += v.z*v.x;
	v.z += v.x*v.y;
	v.x += v.y*v.z;
	v.y += v.z*v.x;
	v.z += v.x*v.y;
	return v >> uvec3(16u);
}


layout(local_size_x = 8, local_size_y = 4) in;
void main()
{
	uint groupId = gl_WorkGroupID.y * gl_NumWorkGroups.x + gl_WorkGroupID.x;
	uint id = groupId * gl_WorkGroupSize.x * gl_WorkGroupSize.y + gl_LocalInvocationIndex;
	if(id >= unitsCount.x)
		return;

	Unit unit = units[id];
	vec3 rand = vec3(RandVector_v1(uvec3(unit.posX, unit.posY, unit.mind[2])));

	vec2 dir = vec2(sin(rand.x), cos(rand.y));
	vec2 pos = vec2(unit.posX, unit.posY);// + dir;

	/*vec2 fieldSize = vec2(imageSize(img_output));
	if(pos.x <= 0.0f || pos.x >= fieldSize.x - 1.0f ||
		pos.y <= 0.0f || pos.y >= fieldSize.y - 1.0f)
	{
		pos = vec2(unit.posX, unit.posY) - dir;
	}*/

	//units[id].posX = uint(pos.x);
	//units[id].posY = uint(pos.y);

	ivec2 pixel_coords = ivec2(pos.x, pos.y);
	vec4 pixel = vec4(id, unit.power, 0.0f, 1.0);
	imageStore(img_output, pixel_coords, pixel);

	/*ivec2 pixel_coords = ivec2(gl_GlobalInvocationID.xy);
	vec4 pixel = vec4(gl_GlobalInvocationID.x, gl_GlobalInvocationID.y, 0.0, 1.0);
	vec2 size = vec2(imageSize(img_output));
	pixel.xy /= size;
	imageStore(img_output, pixel_coords, pixel);*/
}

