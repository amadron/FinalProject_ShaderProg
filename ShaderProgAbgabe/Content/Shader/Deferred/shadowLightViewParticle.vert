#version 430 core
in vec3 position;
in vec3 normal;
in vec2 uv;

in vec3 instancePosition;
in vec3 instanceScale;
in vec3 instanceRotation;

uniform mat4 lightCamera;

out vec4 lightPosition;
out vec4 viewPosition;
out vec2 passUv;

void main()
{
	vec3 npos = position * instanceScale;
	npos += instancePosition;
	vec4 lpos = lightCamera * vec4(npos,1);
	lightPosition = lpos;
	gl_Position = lpos;
	passUv = uv;
}