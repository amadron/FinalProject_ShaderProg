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

/*----------------------------------------------------------------------
*-------------------------------IBL-------------------------------------
*----------------------------Functions----------------------------------
*-----------------------------------------------------------------------
*/

/*
* Function to Create a low discrepancy sequence (Hammersley Sequence)
* which is also uniformly distributed 
* (Used for Quasi Monte Carlo)
* 
*/
float RadicalInverse_VdC(uint bits)
{
	bits = (bits << 16u) | (bits >> 16u);
	bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
	bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
	bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
	bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
	return float(bits) * 2.3283064365386963e-10; // / 0x100000000
}
// ----------------------------------------------------------------------------
vec2 Hammersley(uint i, uint N)
{
	return vec2(float(i) / float(N), RadicalInverse_VdC(i));
}

/*
* Version without bit operator
* Due the fact, that not every driver supports Bitwise operations
*/
float VanDerCorpus(uint n, uint base)
{
	float invBase = 1.0 / float(base);
	float denom = 1.0;
	float result = 0.0;

	for (uint i = 0u; i < 32u; ++i)
	{
		if (n > 0u)
		{
			denom = mod(float(n), 2.0);
			result += denom * invBase;
			invBase = invBase / 2.0;
			n = uint(float(n) / 2.0);
		}
	}

	return result;
}
// ----------------------------------------------------------------------------
vec2 HammersleyNoBitOps(uint i, uint N)
{
	return vec2(float(i) / float(N), VanDerCorpus(i, 2u));
}

/*
*	Function to sample the hemisphere around the 
*	Specular lobe (which is the shape around the reflection distribution)
*	https://learnopengl.com/img/pbr/ibl_specular_lobe.png
*/
vec3 ImportanceSampleGGX(vec2 Xi, vec3 N, float roughness)
{
	float a = roughness * roughness;

	float phi = 2.0 * PI * Xi.x;
	float cosTheta = sqrt((1.0 - Xi.y) / (1.0 + (a*a - 1.0) * Xi.y));
	float sinTheta = sqrt(1.0 - cosTheta * cosTheta);

	// from spherical coordinates to cartesian coordinates
	vec3 H;
	H.x = cos(phi) * sinTheta;
	H.y = sin(phi) * sinTheta;
	H.z = cosTheta;

	// from tangent-space vector to world-space sample vector
	vec3 up = abs(N.z) < 0.999 ? vec3(0.0, 0.0, 1.0) : vec3(1.0, 0.0, 0.0);
	vec3 tangent = normalize(cross(up, N));
	vec3 bitangent = cross(N, tangent);

	vec3 sampleVec = tangent * H.x + bitangent * H.y + N * H.z;
	return normalize(sampleVec);
}