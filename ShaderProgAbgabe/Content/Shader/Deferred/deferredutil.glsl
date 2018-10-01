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

vec3 getHeightMapNormal(sampler2D heightmap, vec2 uv, float scale)
{
	
	float h0 = textureOffset(heightmap, uv, ivec2(0, -1)).r - 0.5;
	float h1 = textureOffset(heightmap, uv, ivec2(-1, 0)).r - 0.5;
	float h2 = textureOffset(heightmap, uv, ivec2(1, 0)).r - 0.5;
	float h3 = textureOffset(heightmap, uv, ivec2(0, 1)).r - 0.5;
	vec3 n;
	n.z = (h0 - h3) * scale;
	n.x = (h1 - h2) * scale;
	n.y = 2;
	return normalize(n);
}

vec3 getCameraUpVector(mat4 camera)
{
	return vec3(camera[0][1],camera[1][1], camera[2][1]);
}

vec3 getCameraRightVector(mat4 camera)
{
	return vec3(camera[0][0], camera[1][0], camera[2][0]);
}

vec3 getBillboardPosition(vec3 position, vec3 scale, vec3 up, vec3 right)
{
	return up * position.y * scale.y  + right * position.x * scale.x;
}

vec4 getConjungatedQuaternion(vec4 quaternion)
{
	vec4 res = normalize(quaternion);
	res.x *= -1;
	res.y *= -1;
	res.z *= -1;
	return res;
}

vec3 rotateByQuaternion(vec4 p, vec4 q)
{
	float x = p.x * (q.x * q.x + q.w * q.w - q.y * q.y - q.z * q.z)
			+ p.y * (2 * q.x * q.y - 2 * q.w * q.z)
			+ p.z * (2 * q.y * q.z + 2 * q.w * q.y);
	float y = p.x * (2 * q.w * q.z + 2 * q.x * q.y)
			+ p.y * (q.w *q.w - q.x * q.x + q.y * q.y - q.z * q.z)
			+ p.z * (-2 * q.w * q.x + 2 * q.y * q.z);
	float z = p.x * (-2 * q.w * q.y + 2 * q.x * q.z)
			+ p.y * (2 * q.w  * q.x + 2 * q.y * q.z)
			+ p.z * (q.w * q.w - q.x * q.x - q.y * q.y + q.z * q.z);
	return vec3(x, y, z);
}
