#version 430 core

vec4 position[] = 
{
	vec4(-1, -1, 0, 1),
	vec4(1, -1, 0, 1),
	vec4(1, 1, 0, 1),
	vec4(-1, 1, 0, 1)
};

out vec2 uv; 

void main()
{
	gl_Position = position[gl_VertexID];
	uv = position[gl_VertexID].xy * 0.5f + 0.5f;
}