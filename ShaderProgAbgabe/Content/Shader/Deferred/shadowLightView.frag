#version 430 core
#include "util.glsl"
uniform int hasAlphaMap;
uniform sampler2D alphaSampler;


in vec4 lightPosition;
in vec2 passUv;

out vec4 color;

float GetDistanceMapping(float dist)
{
	float diff = 1/(50 - 0.1);
	return dist * diff;
}

void main()
{
	float dist = lightPosition.z/lightPosition.w;
	//dist = GetDistanceMapping(dist);
	float k = 80;
	float res = exp(k * dist);
	float alpha = getAlpha(hasAlphaMap, alphaSampler, passUv);
	color = vec4(vec3(res), alpha);
}