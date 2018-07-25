#version 430 core

in vec3 instancePosition;
in float instanceRadius;
in vec4 instanceColor;

uniform mat4 camera;
in vec3 position;
in vec3 normal;

out Data 
{
	vec4 position;
	vec3 normal;
	vec4 lightColor;
} outData;

void main()
{
	outData.position = camera * vec4(position, 1);
	outData.normal = normal;
	outData.lightColor = instanceColor;
}