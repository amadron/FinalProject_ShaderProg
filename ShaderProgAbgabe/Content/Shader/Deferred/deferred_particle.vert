#version 430 core

uniform mat4 camera;


in vec3 position;
in vec3 normal;
in vec2 uv;

in vec3 instancePosition;
in vec3 instanceScale;
in vec3 instanceRotation;
in vec3 instanceColor;

out Data {
	vec4 position;
	vec4 transPos;
	vec3 normal;
	flat uint material;
	vec2 uv;
	vec4 color;
} outdata;

void main()
{
	vec3 npos = position * instanceScale;
	npos += instancePosition;
	vec4 transPos = camera * vec4(npos, 1);
	gl_Position = transPos;
	outdata.position = vec4(npos,1);
	outdata.normal = normal;
	outdata.material = uint(uv.x);;
	outdata.uv = uv;
	outdata.transPos = transPos;
	outdata.color = vec4(instanceColor,1);
}