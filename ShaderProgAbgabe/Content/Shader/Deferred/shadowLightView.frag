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
	float alpha = getAlpha(hasAlphaMap, alphaSampler, passUv);
	if(alpha < 0.1f)
		discard;
	float dist = lightPosition.z/lightPosition.w;
	float k = shadowMapExponent;
	float res = exp(k * dist);
	color = vec4(vec3(res), 1) * (1 - isUnlit);
}