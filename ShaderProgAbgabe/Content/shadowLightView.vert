#version 430 core
uniform mat4 lightCamera;

in vec3 position;
in vec3 normal;

out vec4 lightPosition;
out vec4 viewPosition;

void main()
{
	vec4 lpos = lightCamera * vec4(position,1);
	lightPosition = lpos;
	gl_Position = lpos;
}