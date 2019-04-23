#version 450

uniform ivec4 numBuildings;
uniform vec4 buildingSize;

layout(std430, binding = 0) buffer Lod0Instances
{
    uint lod0instances [];
};

layout(std430, binding = 1) buffer Lod1Instances
{
    uint lod1instances [];
};

layout(std430, binding = 2) buffer Lod2Instances
{
    uint lod2instances [];
};

layout(std430, binding = 3) buffer Lod3Instances
{
    uint lod3instances [];
};

layout(std430, binding = 5) buffer CameraData
{
    mat4 ViewMatrix;
    vec4 CameraPos;
    vec4 CameraDir;
    vec4 CameraUp;
    vec4 FrustumPlanes[6];
    uint forceMode;
    uint padding[3];
};

layout(binding = 0, offset = 4) uniform atomic_uint lod0counter;
layout(binding = 1, offset = 4) uniform atomic_uint lod1counter;
layout(binding = 2, offset = 4) uniform atomic_uint lod2counter;
layout(binding = 3, offset = 4) uniform atomic_uint lod3counter;

layout(local_size_x = 8, local_size_y = 8) in;
void main()
{
    ivec2 pixel_coords = ivec2(gl_GlobalInvocationID.xy);

    vec3 min = vec3(0.0f);
    min.x = float(pixel_coords.x) * (buildingSize.x + buildingSize.y) + buildingSize.y;
    min.y = -buildingSize.x;
    min.z = float(pixel_coords.y) * (buildingSize.x + buildingSize.y) + buildingSize.y;

    vec3 max = vec3(min.x + buildingSize.x, buildingSize.x, min.z + buildingSize.x);
    vec3 pos = vec3(min.x + buildingSize.x * 0.5f, 0.0f, min.z + buildingSize.x * 0.5f);

    uvec3 umin = floatBitsToUint(min);
    uvec3 umax = floatBitsToUint(max);

    bool visible = true;
    for(uint p = 0; p < 6; p++)
    {
        uint negMskX = FrustumPlanes[p].x < 0.0f ? 0xffffffff : 0;
        uint negMskY = FrustumPlanes[p].y < 0.0f ? 0xffffffff : 0;
        uint negMskZ = FrustumPlanes[p].z < 0.0f ? 0xffffffff : 0;

        uvec3 un;
        un.x = (~negMskX & umin.x) | (negMskX & umax.x);
        un.y = (~negMskY & umin.y) | (negMskY & umax.y);
        un.z = (~negMskZ & umin.z) | (negMskZ & umax.z);

        vec3 n = uintBitsToFloat(un);

        float dist = dot(FrustumPlanes[p].xyz, n) - FrustumPlanes[p].w;
        if(dist >= 0.0f)
        {
            visible = false;
            break;
        }
    }

    if (visible)
    {
        float distance = length(pos - CameraPos.xyz);
        if(distance < 32.0f * 20.0f)
        {
            uint index = atomicCounterIncrement(lod0counter);
            lod0instances[index] = pixel_coords.y * numBuildings.x + pixel_coords.x;
        }
        else if (distance < 32.0f * 30.0f)
        {
            uint index = atomicCounterIncrement(lod1counter);
            lod1instances[index] = pixel_coords.y * numBuildings.x + pixel_coords.x;
        }
        else if (distance < 32.0f * 40.0f)
        {
            uint index = atomicCounterIncrement(lod2counter);
            lod2instances[index] = pixel_coords.y * numBuildings.x + pixel_coords.x;
        }
        else
        {
            uint index = atomicCounterIncrement(lod3counter);
            lod3instances[index] = pixel_coords.y * numBuildings.x + pixel_coords.x;
        }
    }
}
