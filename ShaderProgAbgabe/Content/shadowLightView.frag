#version 430 core

in vec4 lightPosition;

out vec4 color;
void main()
{
	float dist = lightPosition.z/lightPosition.w;
	//dist = 0.1f;
	float k = 88;
	float res = exp(k * dist);

	color = vec4(res);
}