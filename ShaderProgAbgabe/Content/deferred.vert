#version 430 core

uniform mat4 camera;

in vec3 position;
in vec3 normal;
in vec2 uv;

out Data {
	vec3 position;
	vec3 normal;
	flat uint material;
} outdata;

void main()
{
	outdata.position = position;
	outdata.normal = normal;
	gl_Position = camera * vec4(position, 1);
	outdata.material = uint(uv.x);;
}