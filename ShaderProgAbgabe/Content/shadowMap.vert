#version 430 core
uniform mat4 camera;
uniform mat4 lightCamera;

in vec3 position;

out vec4 lightPosition;
out vec4 viewPosition;

void main()
{
	vec4 lpos = lightCamera * vec4(position, 1);
	lightPosition = lpos;
	vec4 vpos = camera * vec4(position, 1);
	viewPosition = vpos;
	gl_Position = vpos;
}