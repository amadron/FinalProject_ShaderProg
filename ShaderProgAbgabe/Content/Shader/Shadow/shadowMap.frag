#version 430 core
uniform sampler2D lightViewSampler;

in vec4 lightPosition;
in vec4 viewPosition;

out vec4 color;

float GetDistanceMapping(float dist)
{
	float diff = 1/(50 - 0.1);
	return dist * diff;
}

void main()
{
	vec3 lpos = lightPosition.xyz/lightPosition.w;
	vec3 vpos = viewPosition.xyz/viewPosition.w;
	float vdepth = lpos.z - 0.000001;
	float k = -80;
	float vexp = exp(k * vdepth);
	float lexp = texture(lightViewSampler, lpos.xy * 0.5 + 0.5).r;
	float shadowFact = vexp * lexp;
	float shadow  = clamp(shadowFact, 0, 1);
	color = vec4(shadowFact);
}