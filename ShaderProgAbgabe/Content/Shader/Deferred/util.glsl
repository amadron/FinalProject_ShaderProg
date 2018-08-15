float getAlpha(int hasAlphaMap, sampler2D alphaMap, vec2 uv)
{
	float alpha = texture(alphaMap, uv).r;
	alpha = alpha + 1 - hasAlphaMap;
	return alpha;
}
 