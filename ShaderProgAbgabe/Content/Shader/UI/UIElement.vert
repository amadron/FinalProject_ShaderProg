#version 430 core
uniform vec3 scale;
uniform vec3 position;

vec3 positions[] = 
{
	vec3(-0.5f, -0.5f, 0),
	vec3(0.5f, -0.5f, 0),
	vec3(0.5f, 0.5f, 0),
	vec3(-0.5f, 0.5f, 0)
};

out vec2 uv;


void main()
{
	vec3 pos = positions[gl_VertexID];
	pos *= scale;
	pos.xy += position.xy;
	pos.z -= position.z;
	gl_Position = vec4(pos,1);
	uv = positions[gl_VertexID].xy + 0.5f;
}