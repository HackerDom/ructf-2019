#version 450

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

layout(std430, binding = 8) buffer Units
{
	Unit units[];
};

layout(binding = 2) uniform sampler2D randomTex;

uniform ivec4 unitsCount;
uniform vec4 fieldSize;

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

    vec2 uv = vec2(RandVector_v2(uvec2(unit.mind[0], unit.mind[1])));
    vec4 randf = texture(randomTex, uv);
	uvec2 randu = floatBitsToUint(randf.zw);

	vec2 prevPos = vec2(unit.posX, unit.posZ);
	ivec2 prevUPos = ivec2(prevPos);
	vec2 prevDir = kDirections[unit.prevDirIdx];

	int maskedDirections1 = 0;
	ivec2 prevUPosMod = ivec2(prevUPos.x % 32, prevUPos.y % 32);
	if(prevUPosMod.x > 2 && prevUPosMod.x < 6 && prevUPosMod.y > 2 && prevUPosMod.y < 6 && randf.y > 0.0f)
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

	int maskedDirections = maskedDirections1 & maskedDirections2 & maskedDirections3;

	uint dirIdx = unit.prevDirIdx;
	for(uint i = 0; i < 4; i++)
	{
		uint bit = (randu.x ^ randu.y + i) % 5;
		if((maskedDirections & (1 << bit)) > 0)
		{
			dirIdx = bit;
			break;
		}
	}
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
}

