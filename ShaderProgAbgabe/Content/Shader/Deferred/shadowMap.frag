#version 430 core
#include "deferredutil.glsl"
uniform int shadowMapExponent;
uniform sampler2D lightViewSampler;
uniform sampler2D normalSampler;
uniform int hasAlphaMap;
uniform sampler2D alphaSampler;
uniform float isUnlit;

uniform vec3 lightDirection;

in vec4 lightPosition;
in vec4 viewPosition;
in vec2 outuv;

out vec4 color;

float GetDistanceMapping(float dist)
{
	float diff = 1/(50 - 0.1);
	return dist * diff;
}

void main()
{
	vec3 lpos = lightPosition.xyz/lightPosition.w;
	vec3 vpos = viewPosition.xyz/viewPosition.w;
	vec3 normal = texture(normalSampler, outuv).xyz;
	float cosTheta = dot(normal, -normalize(lightDirection));
	cosTheta = clamp(cosTheta, 0, 1);
	float biasFactor = 0.00000001f;
	float bias = biasFactor * tan(acos(cosTheta));
	bias = clamp(bias, 0f, biasFactor);
	float vdepth = lpos.z - bias;
	float alpha = getAlpha(hasAlphaMap, alphaSampler, outuv);
	float k = -shadowMapExponent;
	float vexp = exp(k * vdepth);
	float lexp = texture(lightViewSampler, lpos.xy * 0.5 + 0.5).r;
	float shadowFact = vexp * lexp;
	float shadow  = clamp(shadowFact, 0, 1); 
	color = vec4(vec3(shadowFact), alpha) * (1 - isUnlit);
}