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
	float vdepth = lpos.z - 0.1;
	vdepth = GetDistanceMapping(vdepth);
	float k = -50;
	vdepth = vdepth * k;
	float vexp = exp(vdepth);
	float lexp = texture(lightViewSampler, lpos.xy * 0.5 + 0.5).r;
	float shadowFact = vexp * lexp;
	float shadow  = clamp(shadowFact, 0,1);
	color = vec4(vec3(shadowFact),1);
}