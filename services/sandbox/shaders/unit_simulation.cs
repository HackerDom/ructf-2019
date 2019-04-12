#version 450

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

layout(rgba16f, binding = 1) uniform image2D img_output;
layout(binding = 2) uniform sampler2D randomTex;
layout(rgba16f, binding = 3) uniform image2D mapImage;

uniform ivec4 unitsCount;

uint hash(uint x, uint y)
{
	const uint M = 1664525u, C = 1013904223u;
	uint seed = (x * M + y + C) * M;
	seed ^= (seed >> 11u);
	seed ^= (seed << 7u) & 0x9d2c5680u;
	seed ^= (seed << 15u) & 0xefc60000u;
	seed ^= (seed >> 18u);
	return seed;
}


uvec2 RandVector_v2(uvec2 p)
{
	uint seed1 = hash(uint(p.x), uint(p.y));
	uint seed2 = hash(seed1, 1000);
	return uvec2( seed1, seed2 );
}


layout(local_size_x = 8, local_size_y = 4) in;
void main()
{
	uint groupId = gl_WorkGroupID.y * gl_NumWorkGroups.x + gl_WorkGroupID.x;
	uint id = groupId * gl_WorkGroupSize.x * gl_WorkGroupSize.y + gl_LocalInvocationIndex;
	if(id >= unitsCount.x)
		return;

	Unit unit = units[id];

	const vec2 kDirections[] = vec2[](vec2(1.0f, 0.0f), vec2(0.0f, 1.0f), vec2(-1.0f, 0.0f), vec2(0.0f, -1.0f));
    vec2 fieldSize = vec2(imageSize(img_output));
	vec2 invFieldSize = vec2(0.5f) / fieldSize;

    vec2 rand = vec2(RandVector_v2(uvec2(floatBitsToUint(unit.posX) | unit.mind[0], floatBitsToUint(unit.posZ) | unit.mind[1])));
	vec2 rand01 = vec2(cos(rand.x * 3.14f), sin(rand.y * 3.14f));
    vec2 randf = texture(randomTex, rand01).xy;
	uvec2 randu = floatBitsToUint(randf);

	vec2 prevPos = vec2(unit.posX, unit.posZ);
	ivec2 prevUPos = ivec2(prevPos);
	vec2 prevDir = kDirections[unit.prevDirIdx];

	int maskedDirections0 = 15;

	int maskedDirections1 = 0;
	if((prevUPos.x % 32) < 8 && (prevUPos.y % 32) < 8 && randf.y > 0.0f)
		maskedDirections1 = 15;

	int maskedDirections2 = 1 << ((unit.prevDirIdx + 2) % 4);
	maskedDirections2 = (~maskedDirections2) & 15;

	uint crossIdx = (prevUPos.x / 32) & 0xffff;
	crossIdx |= ((prevUPos.y / 32) & 0xffff) << 16;
	int maskedDirections3 = 0;
	if(crossIdx != unit.prevCrossIdx && maskedDirections1 > 0)
	{
		maskedDirections3 = 15;
		units[id].prevCrossIdx = crossIdx;
	}

	int maskedDirections = maskedDirections0 & maskedDirections1 & maskedDirections2 & maskedDirections3;
	maskedDirections |= 1 << 4;

	uint dirIdx = unit.prevDirIdx;
	for(uint i = 0; i < 5; i++)
	{
		uint bit = (randu.x + i) % 5;
		if((maskedDirections & (1 << bit)) > 0)
		{
			dirIdx = bit;
			break;
		}
	}
	if(dirIdx == 4)
		dirIdx = unit.prevDirIdx;
	vec2 pos = prevPos + kDirections[dirIdx] * units[id].power;

	if(pos.x < 1.0f)
		dirIdx = 0;
	if(pos.x > fieldSize.x - 1.0f)
		dirIdx = 2;
	if(pos.y < 1.0f)
		dirIdx = 1;
	if(pos.y > fieldSize.y - 1.0f)
		dirIdx = 3;

	units[id].posX = pos.x;
	units[id].posZ = pos.y;
	units[id].prevDirIdx = dirIdx;

    ivec2 pixel_coords = ivec2(pos.x, pos.y);
	vec4 pixel = vec4(0.0f);
	pixel.x = unit.mind[0] & 0xff;
	pixel.y = unit.mind[1] & 0xff;
	pixel.z = unit.mind[2] & 0xff;
	pixel.w = 255.0f;
	pixel /= vec4(255.0f);
	imageStore(img_output, pixel_coords, pixel);
}

