#version 430 core

uniform mat4 camera;
in vec3 position;
in vec3 normal;

in Data 
{
	vec4 position;
	vec3 normal;
	vec4 lightColor;
} inData;

void main()
{
	gl_FragColor = inData.lightColor;
}