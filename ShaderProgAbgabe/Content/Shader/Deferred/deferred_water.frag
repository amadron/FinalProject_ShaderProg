﻿#version 430 core
#include "deferredutil.glsl"

uniform vec3 cameraPosition;
uniform vec3 cameraDirection;

uniform int hasAlbedo;
uniform sampler2D albedoSampler;
uniform int hasNormalMap;
uniform sampler2D normalSampler;
uniform int hasAlphaMap;
uniform sampler2D alphaSampler;
uniform int hasEnvironmentMap;
uniform sampler2D environmentSampler;
uniform float reflectionFactor;

in Data {
	vec4 position;
	vec4 transPos;
	vec3 normal;
	flat uint material;
	vec2 uv;
} inData;

out vec4 position;
out vec4 albedo;
out vec4 normal;

void main()
{
	const vec3 materials[] = { vec3(1), vec3(1, 0, 0), vec3(0, 1, 0), vec3(0, 1, 1) };

	float alpha = getAlpha(hasAlphaMap, alphaSampler, inData.uv);
	vec4 pos = inData.position / inData.position.w;
	pos.a = 1 - step(alpha, 0.9);
	position = pos;
	vec3 inNormal = normalize(inData.normal);
	vec3 normalSum = inNormal * (1 - hasNormalMap) +  -(texture(normalSampler, inData.uv).rgb * 2f - 1f) * hasNormalMap;
	vec4 n = vec4(normalize(normalSum),1);
	n.a = alpha;
	normal = n;

	vec3 viewDir = normalize(inData.position.xyz - cameraPosition);
	vec3 comb = normalize((viewDir + -cameraDirection)/2);
	vec4 environment = getEnvironment(viewDir, n.xyz, environmentSampler) * reflectionFactor;

	vec4 color = vec4(materials[inData.material],1) * (1 - hasAlbedo) +	texture(albedoSampler, inData.uv) * hasAlbedo;
	color = mix(color, environment, reflectionFactor);
	color.a = alpha;
	albedo =  clamp(color, 0, 1);
}