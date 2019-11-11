#version 430 core

uniform vec3 albedo_Color;
uniform float roughness;
uniform float metal;
uniform float ao;

uniform vec3 camPosition;

uniform vec3 lightDirection;

in vec3 pos;
in vec3 fragNormal;
in vec2 fragUV;

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
	float dProd = max(dot(n, h),0);
	float dProdSquare = dProd * dProd; //dot^2
	float roughMinus = roughSqr - 1;	//a^2 - 1

	float clip = dProdSquare * roughMinus + 1; //(dot(n,h)^2(a^2 - 1) + 1
	clip = clip * clip;
	float denom = PI * clip;
	return roughSqr / denom;
}

//GSchlick GGX
float GSub(vec3 n, vec3 v, float k)
{
	float numorator = max(dot(n,v), 0.0f);

	//(n dot v)(1-k)+k
	float denom = numorator * (1-k)+k;
	return numorator / denom;
}

//Geometry Function
//k direct: (a+1) div 8
//k IBL:	a^2 div 2
float GeometryFunction(vec3 n, vec3 v, vec3 l, float k)
{
	float Gview = GSub(n, v, k);
	float Glight = GSub(n, l, k);
	return Gview * Glight;
}

//Fresnel
vec3 Fresnel(vec3 h, vec3 v, vec3 IOR)
{
	float dProd = max(dot(h,v),0.0);
	return IOR + (1-IOR) * pow((1-dProd),5);
}

//---------------------END DGF----------------------------

//TODO
float Radiance(vec3 p, vec3 n, vec3 l, float dW)
{
	return dot(n, l) * dW;
}

void main()
{
	vec3 normal = normalize(fragNormal);
	vec3 viewDir = normalize(camPosition - pos);
	
	fragColor = vec4(albedo_Color,1);
}