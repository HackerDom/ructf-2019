#version 450

/*layout (triangles) in;
layout (line_strip, max_vertices = 4) out;

uniform mat4 viewProjMatrix;
uniform vec4 viewDir;

void EmitLine(int idx0, int idx1)
{
	gl_Position = viewProjMatrix * gl_in[idx0].gl_Position; 
    EmitVertex();

    gl_Position = viewProjMatrix * gl_in[idx1].gl_Position;
    EmitVertex();
    
    EndPrimitive();
}

void main() 
{    
	vec3 edge0 = gl_in[1].gl_Position.xyz - gl_in[0].gl_Position.xyz;
	vec3 edge1 = gl_in[2].gl_Position.xyz - gl_in[0].gl_Position.xyz;
	vec3 edge2 = gl_in[2].gl_Position.xyz - gl_in[1].gl_Position.xyz;
	vec3 normal = cross(edge0, edge1);
	if(dot(normal, -viewDir.xyz) < 0)
		return;


    if(abs(dot(edge0, edge2)) < 0.01f)
	{
		EmitLine(0, 1);
		EmitLine(1, 2);
	}
} */


layout (triangles) in;
layout(triangle_strip, max_vertices=3) out;

out vec3 barycentricCoords;
out vec3 normal;

uniform mat4 projMatrix;

layout(std430, binding = 0) buffer CameraData
{
    mat4 ViewMatrix;
    vec4 CameraPos;
    vec4 CameraDir;
    vec4 CameraUp;
    vec4 FrustumPlanes[6];
	uint forceMode;
    uint padding[3];
};

void main() 
{    
	vec3 edge0 = gl_in[1].gl_Position.xyz - gl_in[0].gl_Position.xyz;
	vec3 edge1 = gl_in[2].gl_Position.xyz - gl_in[0].gl_Position.xyz;
	vec3 edge2 = gl_in[2].gl_Position.xyz - gl_in[1].gl_Position.xyz;
	normal = cross(edge0, edge1);

	mat4 viewProjMatrix = projMatrix * ViewMatrix;

	gl_Position = viewProjMatrix * gl_in[0].gl_Position; 
	barycentricCoords = vec3(1.0f, 0.0f, 0.0f);
	normal = cross(edge0, edge1);
    EmitVertex();

	gl_Position = viewProjMatrix * gl_in[1].gl_Position; 
	barycentricCoords = vec3(0.0f, 1.0f, 0.0f);
	normal = cross(edge0, edge1);
    EmitVertex();

	gl_Position = viewProjMatrix * gl_in[2].gl_Position; 
	barycentricCoords = vec3(0.0f, 0.0f, 1.0f);
	normal = cross(edge0, edge1);
    EmitVertex();

	EndPrimitive();
}