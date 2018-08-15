float getAlpha(int hasAlphaMap, sampler2D alphaMap, vec2 uv)
{
	float alpha = texture(alphaMap, uv).r;
	alpha = alpha + 1 - hasAlphaMap;
	return alpha;
}

const float PI = 3.14159265359;

vec2 projectLongLat(vec3 direction) {
	float theta = atan(direction.x, -direction.z) + PI;
	float phi = acos(-direction.y);
	return vec2(theta / (2*PI), phi / PI);
}
 