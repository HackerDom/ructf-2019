#version 450

struct Building
{
    float posX;
    float posY;
    float sizeX;
    float sizeY;
    vec4 color;
};

layout(std430, binding = 0) buffer Buildings
{
    Building buildings[];
};

uniform ivec4 buildingsCount;

layout(rgba16f, binding = 2) uniform image2D img_output;


layout(local_size_x = 8, local_size_y = 4) in;
void main()
{
	uint groupId = gl_WorkGroupID.y * gl_NumWorkGroups.x + gl_WorkGroupID.x;
	uint id = groupId * gl_WorkGroupSize.x * gl_WorkGroupSize.y + gl_LocalInvocationIndex;
	if(id >= buildingsCount.x)
		return;

    uint startX = uint(buildings[id].posX - buildings[id].sizeX * 0.5f);
    uint startY = uint(buildings[id].posY - buildings[id].sizeY * 0.5f);
    uint sizeX = uint(buildings[id].sizeX);
    uint sizeY = uint(buildings[id].sizeY);

    for(uint x = 0; x < sizeX; x++)
    {
        for (uint y = 0; y < sizeY; y++)
        {
            ivec2 pixel_coords = ivec2(x + startX, y + startY);
            imageStore(img_output, pixel_coords, buildings[id].color);
        }
    }
}

