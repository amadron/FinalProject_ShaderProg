#version 430 core

uniform mat4 camera;

in vec3 position;
in vec3 normal;
in vec2 uv;

out Data {
	vec4 position;
	vec3 normal;
	flat uint material;
} outdata;

void main()
{
	vec4 transPos = camera * vec4(position, 1);
	gl_Position = transPos;
	outdata.position = vec4(position,1);
	outdata.normal = normal;
	outdata.material = uint(uv.x);;
}