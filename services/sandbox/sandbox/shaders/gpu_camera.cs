#version 450

layout(std430, binding = 0) buffer CameraData
{
    mat4 ViewMatrix;
    vec4 CameraPos;
    vec4 CameraDir;
    vec4 CameraUp;
    vec4 FrustumPlanes[6];
    uint ForceMode;
    uint padding[3];
};
uniform vec4 ProjData;


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

layout(std430, binding = 1) buffer Units
{
    Unit units [];
};
uniform ivec4 UnitIdxToFollow;


vec4 BuildFrustumPlane(vec3 n, mat4 viewMatrix, vec3 cameraPos, float offset)
{
	vec3 xn = normalize(n * mat3(viewMatrix));
	return vec4(xn.x, xn.y, xn.z, dot(cameraPos.xyz, xn) + offset);
}


vec4 BuildFrustumPlane(vec3 v[3], mat4 viewMatrix, vec3 cameraPos, float offset)
{
	vec3 n = -cross((v[0] - v[1]), (v[2] - v[1]));
	return BuildFrustumPlane(n, viewMatrix, cameraPos, offset);
}


#define kTop 0
#define kBottom 1
#define kLeft 2
#define kRight 3
#define kFar 4
#define kNear 5


void BuildFrustumPlanes(vec3 cameraPos, mat4 viewMatrix, float fovy, float aspect, float znear, float zfar)
{
	vec3 v[3];
	float rx, ry;

	float tanFovY = tan(fovy * 0.5f);
	float tanFovX = tanFovY * aspect;
	
	rx = tanFovX * znear;
	ry = tanFovY * znear;

	v[0] = vec3(0.0f, 0.0f, 0.0f);

	// top (+)
	v[2] = vec3(-rx, +ry, znear);
	v[1] = vec3(+rx, +ry, znear);
	FrustumPlanes[kTop] = BuildFrustumPlane(v, viewMatrix, cameraPos, 0);

	// bottom (+)
	v[1] = vec3(-rx, -ry, znear);
	v[2] = vec3(+rx, -ry, znear);
	FrustumPlanes[kBottom] = BuildFrustumPlane(v, viewMatrix, cameraPos, 0);

	// left (+)
	v[2] = vec3(-rx, -ry, znear);
	v[1] = vec3(-rx, +ry, znear);
	FrustumPlanes[kLeft] = BuildFrustumPlane(v, viewMatrix, cameraPos, 0);

	// right (+)
	v[1] = vec3(+rx, -ry, znear);
	v[2] = vec3(+rx, +ry, znear);
	FrustumPlanes[kRight] = BuildFrustumPlane(v, viewMatrix, cameraPos, 0);

	// farthest (+)
	v[0] = vec3(0.0f, 0.0f, -1.0f);
	FrustumPlanes[kFar] = BuildFrustumPlane(v[0], viewMatrix, cameraPos, zfar);

	// back (+)
	v[0] = vec3(0.0f, 0.0f, +1.0f);
	FrustumPlanes[kNear] = BuildFrustumPlane(v[0], viewMatrix, cameraPos, -znear);
}


mat4 BuildViewMatrix(vec3 pos, vec3 dir, vec3 up)
{
    vec3 center = pos + dir;

    vec3 f = normalize(center - pos);
	vec3 s = normalize(cross(f, up));
    vec3 u = cross(s, f);

    mat4 result = mat4(0.0f);
    result[0][0] = s.x;
    result[1][0] = s.y;
    result[2][0] = s.z;
    result[0][1] = u.x;
    result[1][1] = u.y;
    result[2][1] = u.z;
    result[0][2] =-f.x;
    result[1][2] =-f.y;
    result[2][2] =-f.z;
    result[3][0] =-dot(s, pos);
    result[3][1] =-dot(u, pos);
    result[3][2] = dot(f, pos);
    result[3][3] = 1.0f;

    return result;
}


layout(local_size_x = 1, local_size_y = 1) in;
void main()
{
    mat4 viewMatrix = ViewMatrix;
    vec3 cameraPos = CameraPos.xyz;
    vec3 cameraDir = CameraDir.xyz;
    vec3 cameraUp = CameraUp.xyz;

    if(ForceMode == 0)
    {
        cameraUp = vec3(0.0f, 1.0f, 0.0f);
        if(UnitIdxToFollow.x >= 0)
        {
            Unit unit = units[UnitIdxToFollow.x];
            vec3 unitPos = vec3(unit.posX, unit.posY, unit.posZ);
            cameraDir = normalize(unitPos - cameraPos);
            cameraPos = (unitPos - cameraDir * 5.0f);
        }
    }

    viewMatrix = BuildViewMatrix(cameraPos, cameraDir, cameraUp);

    BuildFrustumPlanes(cameraPos, viewMatrix, ProjData.x, ProjData.y, ProjData.z, ProjData.w);
    ViewMatrix = viewMatrix;
    CameraPos.xyz = cameraPos;
    CameraDir.xyz = cameraDir;
    CameraUp.xyz = cameraUp;
}