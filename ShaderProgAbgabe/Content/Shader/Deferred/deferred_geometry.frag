#version 430 core
#include "util.glsl"

uniform vec3 camPos;

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
	//vec3 pos = inData.position;
	
	//float alpha = texture(alphaSampler, inData.uv).r;
	//alpha = alpha + 1 - hasAlphaMap;
	float alpha = getAlpha(hasAlphaMap, alphaSampler, inData.uv);
	vec4 pos = inData.position / inData.position.w;
	pos.a = 1 - step(alpha, 0.9);
	position = pos;
	
	vec3 n = inData.normal * (1 - hasNormalMap) + texture(normalSampler, inData.uv).rgb * hasNormalMap;
	n = normalize(n);
	
	normal = vec4(n,alpha);

	vec3 camDir = normalize(camPos - inData.transPos.xyz);
	vec3 envDir = normalize(reflect(camDir, n));
	vec4 environment = texture(environmentSampler, projectLongLat(envDir));

	vec4 color = vec4(materials[inData.material],1) * (1 - hasAlbedo) +	texture(albedoSampler, inData.uv) * hasAlbedo;
	color = color + environment * hasEnvironmentMap;
	color.a = alpha;
	albedo = color;
}