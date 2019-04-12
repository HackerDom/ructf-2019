#version 450

layout(rgba16f, binding = 2) uniform image2D img_output;

layout(local_size_x = 8, local_size_y = 8) in;
void main()
{
	ivec2 pixel_coords = ivec2(gl_GlobalInvocationID.xy);
	imageStore(img_output, pixel_coords, vec4(0.0f));
}
