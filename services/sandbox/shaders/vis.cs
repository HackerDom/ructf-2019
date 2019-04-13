#version 450

uniform ivec4 numBuildings;
uniform vec4 buildingSize;
uniform vec4 frustumPlanes[6];

layout(std430, binding = 8) buffer Instances
{
    uint instances[];
};

layout(binding = 1, offset = 4) uniform atomic_uint counter;

layout(local_size_x = 8, local_size_y = 8) in;
void main()
{
    ivec2 pixel_coords = ivec2(gl_GlobalInvocationID.xy);

    vec3 min = vec3(0.0f);
    min.x = float(pixel_coords.x) * (buildingSize.x + buildingSize.y) + buildingSize.y;
    min.y = -buildingSize.x;
    min.z = float(pixel_coords.y) * (buildingSize.x + buildingSize.y) + buildingSize.y;

    vec3 max = vec3(min.x + buildingSize.x, buildingSize.x, min.z + buildingSize.x);

    uvec3 umin = floatBitsToUint(min);
    uvec3 umax = floatBitsToUint(max);

    bool visible = true;
    for(uint p = 0; p < 6; p++)
    {
        uint negMskX = frustumPlanes[p].x < 0.0f ? 0xffffffff : 0;
        uint negMskY = frustumPlanes[p].y < 0.0f ? 0xffffffff : 0;
        uint negMskZ = frustumPlanes[p].z < 0.0f ? 0xffffffff : 0;

        uvec3 un;
        un.x = (~negMskX & umin.x) | (negMskX & umax.x);
        un.y = (~negMskY & umin.y) | (negMskY & umax.y);
        un.z = (~negMskZ & umin.z) | (negMskZ & umax.z);

        vec3 n = uintBitsToFloat(un);

        float dist = dot(frustumPlanes[p].xyz, n) - frustumPlanes[p].w;
        if(dist >= 0.0f)
        {
            visible = false;
            break;
        }
    }

    if (visible)
    {
        uint index = atomicCounterIncrement(counter);
        instances[index] = pixel_coords.y * numBuildings.x + pixel_coords.x;
    }
}
