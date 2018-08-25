#version 430 core
#include "waterutil.glsl"

vec4 position[] = 
{
	vec4(-1, -1, 0, 1),
	vec4(1, -1, 0, 1),
	vec4(1, 1, 0, 1),
	vec4(-1, 1, 0, 1)
};

in vec3 normal;
in vec2 uv;

out Data {
	vec3 position;
	vec3 normal;
	vec2 uv;
} outData;

void main()
{
	gl_Position = position[gl_VertexID];
	outData.position = position[gl_VertexID].xyz;
	outData.uv = position[gl_VertexID].xy * 0.5f + 0.5f;
}