#version 450

struct Unit
{
	uint mind[8];
	uint id;
	float posX;
	float posY;
	uint type;
	float power;
    vec2 prevDir;
};

layout(std430, binding = 0) buffer Units
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

    vec2 rand = vec2(RandVector_v2(uvec2(floatBitsToUint(unit.posX) | unit.mind[0], floatBitsToUint(unit.posY) | unit.mind[1])));
    vec2 rand01 = vec2(sin(rand.x), cos(rand.y));
    vec2 randf = texture(randomTex, rand01).xy;

    vec2 dir = vec2(0);
    if (dot(unit.prevDir, unit.prevDir) < 0.001f)
        dir = normalize(randf);// floatBitsToUint(rand01.x) % 4;
    else
        dir = unit.prevDir;

    vec2 pos = vec2(0.0f);
    while(true)
    {
        //vec2 dir = kDirections[dirIdx];
        pos = vec2(unit.posX, unit.posY) + dir;

        if (pos.x < 0.0f || pos.x >= fieldSize.x - 1.0f ||
            pos.y < 0.0f || pos.y >= fieldSize.y - 1.0f)
        {
            //dirIdx = (dirIdx + 1) % 4;
            dir = -dir;
            continue;
        }

        uvec4 map = floatBitsToUint(imageLoad(mapImage, ivec2(pos)));
        if((map.x | map.y | map.z) > 0)
        {
            //dirIdx = (dirIdx + 1) % 4;
            dir = -dir;
            continue;
        }

        break;
    }

    /*vec2 rand01 = vec2(sin(rand.x), cos(rand.y));

	//vec2 dir = sign(dirf);

	vec2 pos = vec2(unit.posX, unit.posY) + dir;

	vec2 fieldSize = vec2(imageSize(img_output));
	if(pos.x <= 0.0f || pos.x >= fieldSize.x - 1.0f ||
		pos.y <= 0.0f || pos.y >= fieldSize.y - 1.0f)
	{
		pos = vec2(unit.posX, unit.posY) - dir;
	}*/

    pos += randf * vec2(0.01f);
    units[id].posX = pos.x;
	units[id].posY = pos.y;
    units[id].prevDir = dir;

    ivec2 pixel_coords = ivec2(pos.x, pos.y);
	vec4 pixel = vec4(0.0f);
	pixel.x = unit.mind[0] & 0xff;
	pixel.y = unit.mind[1] & 0xff;
	pixel.z = unit.mind[2] & 0xff;
	pixel.w = 255.0f;
	pixel /= vec4(255.0f);
	imageStore(img_output, pixel_coords, pixel);
}

