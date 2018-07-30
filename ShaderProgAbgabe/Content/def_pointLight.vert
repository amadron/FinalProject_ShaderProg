#version 430 core

in vec3 instancePosition;
in float instanceRadius;
in vec4 instanceColor;
in float instanceIntensity;

uniform mat4 camera;
in vec3 position;
in vec3 normal;

out Data 
{
	vec4 position;
	vec3 normal;
	vec4 lightColor;
	vec3 lightPosition;
	float radius;
	float intensity;
} outData;

void main()
{
	vec3 tpos = position * instanceRadius + instancePosition;
	vec4 pos = camera * vec4(tpos, 1);
	gl_Position = pos;
	outData.position = pos;
	outData.normal = normal;
	outData.lightColor = instanceColor;
	outData.lightPosition = instancePosition;
	outData.radius = instanceRadius;
	outData.intensity = instanceIntensity;
}