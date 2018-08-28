#version 430 core
#include "deferredutil.glsl"
uniform int shadowMapExponent;
uniform int hasAlphaMap;
uniform sampler2D alphaSampler;
uniform float isUnlit;

in vec4 lightPosition;
in vec2 passUv;

out vec4 color;

void main()
{

	float dist = lightPosition.z/lightPosition.w;
	float k = shadowMapExponent;
	float res = exp(k * dist);
	float alpha = getAlpha(hasAlphaMap, alphaSampler, passUv);
	color = vec4(vec3(res), alpha);
}