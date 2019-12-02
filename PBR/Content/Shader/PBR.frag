#version 430 core

uniform vec3 albedo;
uniform float roughness;
uniform float metal;
uniform float ao;

uniform vec3 camPosition;

uniform vec3 lightPosition;
uniform vec3 lightColor;

in vec3 worldPos;
in vec3 Normal;
in vec2 UV;

struct PointLight 
{
	vec3 position;
	float radius;
	vec3 color;
	float offset; //Block alignment in uniform shader blocks
};

layout (std140) uniform BufferPointLights
{
	PointLight pointLight[];
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

	//Denominator
	//PI((dot(n,h)^2(a^2 - 1) + 1)^2
	float dProd = max(dot(n, h),0.0);
	float dProdSquare = dProd * dProd; //dot^2
	float roughMinus = roughSqr - 1.0;	//a^2 - 1

	float clip = dProdSquare * roughMinus + 1.0; //(dot(n,h)^2(a^2 - 1) + 1
	clip = clip * clip;
	float denom = PI * clip;
	return roughSqr / denom;
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
	return IOR + (1-IOR) * pow((1-dProd),5);
}

void main()
{
	vec3 normal = normalize(Normal);
	vec3 viewDir = normalize(camPosition - worldPos);
	
	vec3 IOR = vec3(0.04);
	IOR = mix(IOR, albedo, metal);

	vec3 Lo = vec3(0.0);
	for(int i = 0; i < 1; i++)
	{
		PointLight pLight = pointLight[0];
		//------------------Per Light---------------------
		//radiance
		float lightDist = length(worldPos - pLight.position);
		float dist = lightDist;
		float attenuation = 1.0 / (dist * dist);
		//attenuation = clamp(attenuation, 0, pLight.radius);
		vec3 radiance = pLight.color * attenuation;

		//BRDF
		vec3 lightDir = normalize(worldPos - pLight.position);
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
		float viewAngle = max(dot(normal, viewDir), 0.0);
		Lo  += (refraction * albedo / PI + specular) * radiance * viewAngle;
	}
	//----------------End Per Light------------------
	vec3 ambient = vec3(0.005) * albedo * ao;
	vec3  color = ambient + Lo;

	//HDR Color mapping
	color = color / (color + vec3(1.0)); 
	color = pow(color, vec3(1.0/2.2));
	//fragColor = vec4(albedo,1.0);
	fragColor = vec4(color, 1.0);
	//fragColor = vec4(vec3(normal), 1.0);
	//fragColor = vec4(vec3(ambient),1.0);
	//fragColor = vec4(pointLight[3].color, 1);
	//fragColor = vec4(vec3(worldPos - pointLight[0].position),1);
}