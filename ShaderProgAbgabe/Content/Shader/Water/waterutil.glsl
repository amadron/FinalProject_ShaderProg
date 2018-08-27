
vec3 getWavePosition(vec3 position, float steepness, float amplitude, float wavelength,vec2 direction, float speed, float time)
{
	float waveheight = cos(dot(wavelength * position.xy, direction) + speed * time);
	float x = steepness * amplitude * direction.x * waveheight;
	float y = steepness * amplitude * direction.y * waveheight;
	float z = amplitude * sin(dot(wavelength * position.xy, direction) + speed * time);
	return vec3(x,y,z);
}

vec3 getBitangent(vec3 wavepos, float steepness, vec2 direction, float WA, float s, float c)
{
	float x = float(1) - steepness * pow(direction.x, 2) * WA * s;
	float y = -(steepness * direction.x * direction.y * WA * s);
	float z = direction.x * WA * c;
	return vec3(x,y,z);
}

vec3 getTangent(vec3 wavepos, float steepness, vec2 direction, float WA, float s, float c)
{
	float x = -(steepness * direction.x * direction.y * WA * s);
	float y = 1 - (steepness * pow(direction.y, 2) * WA * s);
	float z = direction.y * WA * c;
	return vec3(x,y,z);
}

vec3 getWaveNormal(float steepness, vec2 direction, float WA, float s, float c)
{
	float x = direction.x * WA * c;
	float y = direction.y * WA * c;
	float z = steepness * WA * s;

	return vec3(x,y,z);
}

