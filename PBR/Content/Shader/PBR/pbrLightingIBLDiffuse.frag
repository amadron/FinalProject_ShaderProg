﻿#version 430 core

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

in Fragment_Data {
	vec3 worldPos;
	vec3 Normal;
	vec2 UV;
	mat3 tbn;
};

uniform samplerCube irradianceMap;

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

const float PI = 3.14159265359;
//------------------------------------------
//-----------DGF FUNCTIONS------------------
//------------------------------------------
//D: Normal Distribution Function:	Amount of Microfacets aligned to Halfway vector
//F: Fresnel Function:				Ratio of surface reflaction at different surface angles
//G: Geometry function:				Self shadowing of the microfacets

//Normal Distribution Function
float NDF(vec3 n, vec3 h, float roughness)
{
	//Numorator
	//a^2
	float roughSqr = roughness * roughness;
	float num = roughSqr * roughSqr;
	//Denominator
	//PI((dot(n,h)^2(a^2 - 1) + 1)^2
	float dProd = max(dot(n, h),0.0);
	float dProdSquare = dProd * dProd; //dot^2

	float roughMinus = num - 1.0;	//a^2 - 1
	float denom = dProdSquare * roughMinus + 1.0; //(dot(n,h)^2(a^2 - 1) + 1
	denom = PI * denom * denom;
	return num / denom;
}

//GSchlick GGX
float GSub(vec3 n, vec3 v, float roughness)
{
	float numorator = max(dot(n,v), 0.0f);
	float r = (roughness + 1.0);
	float k = (r * r) / 8.0;
	//(n dot v)(1-k)+k
	float denom = numorator * (1.0 - k)+k;
	return numorator / denom;
}

//Geometry Function
//k direct: (a+1) div 8
//k IBL:	a^2 div 2
float GeometryFunction(vec3 n, vec3 viewDir, vec3 lightDir, float roughness)
{
	float Gview = GSub(n, viewDir, roughness);
	float Glight = GSub(n, lightDir, roughness);
	return Gview * Glight;
}

//Fresnel
vec3 Fresnel(vec3 h, vec3 v, vec3 IOR)
{
	float dProd = max(dot(h,v),0.0);
	dProd = min(dProd, 1.0);
	return IOR + (1.0-IOR) * pow((1-dProd),5.0);
}

vec3 FresnelWightRoughness(vec3 h, vec3 v, vec3 IOR, float roughness)
{
	float dProd = max(dot(h,v),0.0);
	dProd = min(dProd, 1.0);
	return IOR + (max(vec3(1.0-roughness), IOR) - IOR) * pow((1-dProd),5.0);
}

vec3 GetMapNormal()
{
	vec3 result = texture(normalMap, UV).rgb * 2.0 - 1.0;
	result = normalize(result);
	result = normalize(tbn * result);
	return result;
}

void main()
{
	vec3 normal = normalize(Normal) * (1.0 - hasNormalMap);
	normal += GetMapNormal() * hasNormalMap; 
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

	vec3 irradiance = texture(irradianceMap, normal).rgb;

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
		//ndf = DistributionGGX(normal, halfWayVec, roughness);
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
		//visual = tmpLo;
		//visual = vec3(fresnel);
		
	}
	//----------------End Per Light------------------
	//vec3 ambient = vec3(0.03);
	//Calculate the ambient color by taking the irradiance map
	vec3 specularFact = FresnelWightRoughness(viewDir, normal, IOR, roughness);
	vec3 diffuseFact = vec3(1.0) - specularFact;
	vec3 diffuse = irradiance * albedo;
	vec3 ambient = (diffuseFact * diffuse) *ao; //Take only the diffuse part into account

	ambient *= albedo * ao;
	vec3  color = ambient + Lo;

	//HDR Color mapping
	color = color / (color + vec3(1.0)); 
	color = pow(color, vec3(1.0/2.2));
	fragColor = vec4(color, 1.0);
	//fragColor = vec4(visual, 1.0);
}