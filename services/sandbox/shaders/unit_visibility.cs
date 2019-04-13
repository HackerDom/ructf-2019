#version 450

uniform ivec4 unitsCount;
uniform vec4 frustumPlanes[6];

layout(std430, binding = 0) buffer Instances
{
    uint instances [];
};

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

layout(std430, binding = 1) buffer Units
{
    Unit units [];
};

layout(binding = 0, offset = 4) uniform atomic_uint counter;

layout(local_size_x = 8, local_size_y = 4) in;
void main()
{
    uint groupId = gl_WorkGroupID.y * gl_NumWorkGroups.x + gl_WorkGroupID.x;
    uint id = groupId * gl_WorkGroupSize.x * gl_WorkGroupSize.y + gl_LocalInvocationIndex;
    if (id >= unitsCount.x)
        return;

    Unit unit = units[id];
    vec3 pos = vec3(unit.posX, unit.posY, unit.posZ);

    bool visible = true;
    for (uint p = 0; p < 6; p++)
    {
        float dist = dot(frustumPlanes[p].xyz, pos) - frustumPlanes[p].w;
        if (dist >= 0.0f)
        {
            visible = false;
            break;
        }
    }


    if (visible)
    {
        uint index = atomicCounterIncrement(counter);
        instances[index] = id;
    }
}
