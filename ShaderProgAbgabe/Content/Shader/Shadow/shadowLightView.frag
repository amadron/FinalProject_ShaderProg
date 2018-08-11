#version 430 core


in vec4 lightPosition;

out vec4 color;

float GetDistanceMapping(float dist)
{
	float diff = 1/(50 - 0.1);
	return dist * diff;
}

void main()
{
	float dist = lightPosition.z/lightPosition.w;
	dist = GetDistanceMapping(dist);
	float k = 50;
	dist = k * dist;
	float res = exp(dist);

	color = vec4(res);
}