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

	int startX = int(buildings[id].posX - buildings[id].sizeX * 0.5f);
	int startY = int(buildings[id].posY - buildings[id].sizeY * 0.5f);
	int sizeX = int(buildings[id].sizeX);
	int sizeY = int(buildings[id].sizeY);


	for(int x = 1; x < sizeX - 1; x++)
    {
		for (int y = 1; y < sizeY - 1; y++)
        {
            ivec2 pixel_coords = ivec2(x + startX, y + startY);
            imageStore(img_output, pixel_coords, buildings[id].color);
        }
	}

	vec4 color = buildings[id].color;

	for(int x = 0; x < sizeX; x++)
	{
		color.w = 1.0f;
		ivec2 pixel_coords = ivec2(x + startX, startY);
		imageStore(img_output, pixel_coords, color);
	}

	for (int y = 1; y < sizeY - 1; y++)
	{
		color.w = 2.0f;
		ivec2 pixel_coords = ivec2(sizeX - 1 + startX, y + startY);
		imageStore(img_output, pixel_coords, color);
	}

	for(int x = 0; x < sizeX; x++)
	{
		color.w = 3.0f;
		ivec2 pixel_coords = ivec2(x + startX, sizeY - 1 + startY);
		imageStore(img_output, pixel_coords, color);
	}

	for (int y = 1; y < sizeY - 1; y++)
	{
		color.w = 4.0f;
		ivec2 pixel_coords = ivec2(startX, y + startY);
		imageStore(img_output, pixel_coords, color);
	}
}

