vec4 getDiffuse(vec3 lightDirection, vec3 normal, vec4 lightColor, vec4 albedo, float intensity)
{
	vec3 l = normalize(-lightDirection);
	float lambert = max(0, dot(normal, l));
	return lightColor * lambert * albedo * intensity;

}

vec4 getSpecular(vec3 viewDir, vec3 normal, vec3 lightDirection, vec4 SpecularColor, float specularFactor, float specularIntensity)
{
	vec3 l = normalize(-lightDirection);
	vec3 r = reflect(l, normal) ;
	vec3 v = viewDir;
	float spec = max(0, dot(r, v)); 
	return  SpecularColor * max(0, pow(spec, specularFactor)) * specularIntensity;
}


float getAlpha(int hasAlphaMap, sampler2D alphaMap, vec2 uv)
{
	float alpha = texture(alphaMap, uv).r;
	alpha = 1 - hasAlphaMap + alpha;
	return alpha;
}

const float PI = 3.14159265359;

vec2 projectLongLat(vec3 direction) {
	float theta = atan(direction.x, -direction.z) + PI;
	float phi = acos(-direction.y);
	return vec2(theta / (2*PI), phi / PI);
}
 
vec4 getEnvironment(vec3 cameraDirection, vec3 normal, sampler2D environmentSampler)
{
	vec3 envDir = normalize(reflect(cameraDirection, normal));
	vec4 environment = texture(environmentSampler, projectLongLat(envDir));
	return environment;
}

float mapDepthToRange(float value, float nearPlane, float farPlane)
{
	float dist = farPlane - nearPlane;
	float part = float(1)/dist;
	return value * part;
}