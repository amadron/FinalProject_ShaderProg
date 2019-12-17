//------------------------------------------
//-----------DGF FUNCTIONS------------------
//------------------------------------------
//D: Normal Distribution Function:	Amount of Microfacets aligned to Halfway vector
//F: Fresnel Function:				Ratio of surface reflaction at different surface angles
//G: Geometry function:				Self shadowing of the microfacets
const float PI = 3.14159265359;
//Normal Distribution Function
float NDF(vec3 n, vec3 h, float roughness)
{
	//Numorator
	//a^2
	float roughSqr = roughness * roughness;
	float num = roughSqr * roughSqr;
	//Denominator
	//PI((dot(n,h)^2(a^2 - 1) + 1)^2
	float dProd = max(dot(n, h), 0.0);
	float dProdSquare = dProd * dProd; //dot^2

	float roughMinus = num - 1.0;	//a^2 - 1
	float denom = dProdSquare * roughMinus + 1.0; //(dot(n,h)^2(a^2 - 1) + 1
	denom = PI * denom * denom;
	return num / denom;
}

//GSchlick GGX
float GSub(vec3 n, vec3 v, float roughness)
{
	float numorator = max(dot(n, v), 0.0f);
	float r = (roughness + 1.0);
	float k = (r * r) / 8.0;
	//(n dot v)(1-k)+k
	float denom = numorator * (1.0 - k) + k;
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
	float dProd = max(dot(h, v), 0.0);
	dProd = min(dProd, 1.0);
	return IOR + (1.0 - IOR) * pow((1 - dProd), 5.0);
}

vec3 FresnelWightRoughness(vec3 h, vec3 v, vec3 IOR, float roughness)
{
	float dProd = max(dot(h, v), 0.0);
	dProd = min(dProd, 1.0);
	return IOR + (max(vec3(1.0 - roughness), IOR) - IOR) * pow((1 - dProd), 5.0);
}

vec3 GetMapNormal(mat3 tbn, sampler2D normalMap, vec2 UV)
{
	vec3 result = texture(normalMap, UV).rgb * 2.0 - 1.0;
	result = normalize(result);
	result = normalize(tbn * result);
	return result;
}