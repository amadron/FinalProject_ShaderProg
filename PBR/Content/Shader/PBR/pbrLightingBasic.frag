﻿#version 430 core
#include "PBRUtils.glsl"

uniform vec3 albedoColor;
uniform sampler2D albedoMap;
uniform int hasAlbedoMap;

uniform sampler2D normalMap;
uniform int hasNormalMap;

uniform float roughnessFactor;
uniform sampler2D roughnessMap;
uniform int hasRoughnessMap;

uniform float metalFactor;
uniform sampler2D metallicMap;
uniform int hasMetallicMap;

uniform float aoFactor;
uniform sampler2D occlusionMap;
uniform int hasOcclusionMap;

uniform vec3 camPosition;

uniform vec3 lightPosition;
uniform vec3 lightColor;

in Fragment_Data {
	vec3 worldPos;
	vec3 Normal;
	vec2 UV;
	mat3 tbn;
};


struct PointLight 
{
	vec3 position;
	float radius;
	vec3 color;
	float offset; //Block alignment in uniform shader blocks
};

layout (std140) uniform BufferPointLights
{
	PointLight pointLight[10];
};

uniform int pointLightAmount;

out vec4 fragColor;

void main()
{
	vec3 normal = normalize(Normal) * (1.0 - hasNormalMap);
	normal += GetMapNormal(tbn, normalMap, UV) * hasNormalMap; 
	vec3 viewDir = normalize(camPosition - worldPos);
	
	vec3 albedo = albedoColor * (1.0 - hasAlbedoMap);
	vec4 albedoMapTex = texture(albedoMap, UV);
	albedo += pow(albedoMapTex.xyz, vec3(2.2)) * hasAlbedoMap;
	
	float ao = aoFactor * (1.0 - hasOcclusionMap);
	vec4 occlusionMapTex = texture(occlusionMap, UV);
	ao += occlusionMapTex.r * hasOcclusionMap;

	float metal = metalFactor * (1.0 - hasMetallicMap);
	vec4 metallicMapTex = texture(metallicMap, UV);
	metal += metallicMapTex.r * hasMetallicMap;

	float roughness = roughnessFactor * (1.0 - hasRoughnessMap);
	vec4 roughnessMapTex = texture(roughnessMap, UV);
	roughness += roughnessMapTex.r * hasRoughnessMap;

	vec3 IOR = vec3(0.04);
	IOR = mix(IOR, albedo, metal);

	vec3 Lo = vec3(0.0);
	vec3 visual = vec3(0);
	for(int i = 0; i < pointLightAmount; i++)
	{
		PointLight pLight = pointLight[i];
		//------------------Per Light---------------------
		//radiance
		vec3 lightDir = normalize(pLight.position - worldPos);
		float lightDist = length(worldPos - pLight.position);
		float dist = lightDist;
		float attenuation = 1.0 / (dist * dist);
		//attenuation = clamp(attenuation, 0, pLight.radius);
		vec3 radiance = pLight.color * attenuation;

		//BRDF
		vec3 halfWayVec = normalize(lightDir + viewDir);
	
		float ndf = NDF(normal, halfWayVec, roughness);
		float geometry = GeometryFunction(normal, viewDir, lightDir, roughness);
		vec3 fresnel = Fresnel(halfWayVec, viewDir, IOR);

		//Get the factors for Reflectivity and refraction (Energy conversion)
		vec3 reflectivity = fresnel;
		vec3 refraction = vec3(1.0) - reflectivity;
		refraction *= 1.0 - metal; //why?

		vec3 num = ndf * geometry * fresnel;
		float denom = 4.0 * max(dot(normal, viewDir), 0.0) * max(dot(normal, lightDir),0.0);
		vec3 specular = num / max(denom, 0.001);

		//specular = num/denom;
		float lightAngle = max(dot(normal, lightDir), 0.0);
		vec3 tmpLo  = (refraction * albedo / PI + specular) * radiance * lightAngle;
		Lo += tmpLo;
		visual = vec3(specular);
		//visual = vec3();
	}
	//----------------End Per Light------------------
	vec3 ambient = vec3(0.03);
	//vec3 ambient = irradiance;
	ambient *= albedo * ao;
	vec3  color = ambient + Lo;

	//HDR Color mapping
	color = color / (color + vec3(1.0)); 
	color = pow(color, vec3(1.0/2.2));
	fragColor = vec4(color, 1.0);
	//fragColor = vec4(visual, 1.0);
}