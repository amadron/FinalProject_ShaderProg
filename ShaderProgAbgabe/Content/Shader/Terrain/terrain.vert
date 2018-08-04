#version 430 core
uniform mat4x4 camera;

uniform sampler2D heightMap;
uniform float heightScale;

in vec3 position;
in vec3 normal;
in vec2 uv;

out Data {
	vec4 position;
	vec3 normal;
	vec2 uv;
} outdata;

void main()
{
	//0 is normal height
	float height = 0.5f - texture(heightMap, uv).r;
	vec3 npos = position;
	npos.y += height;
	vec4 outpos = camera * vec4(npos,1);
	outdata.uv = uv;
	outdata.position = outpos;
	gl_Position = outpos;

}