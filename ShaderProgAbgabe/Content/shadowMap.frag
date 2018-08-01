#version 430 core
uniform sampler2D lightViewSampler;

in vec4 lightPosition;
in vec4 viewPosition;

out vec4 color;

void main()
{
	vec3 lpos = lightPosition.xyz/lightPosition.w;
	vec3 vpos = viewPosition.xyz/viewPosition.w;
	float vdepth = lpos.z - 0.00001f;
	float k = -88;
	float vexp = exp(k * vdepth);
	float lexp = texture(lightViewSampler, lpos.xy * 0.5 + 0.5).r;
	float shadowFact = vexp * lexp;
	float shadow  = clamp(0, 1, shadowFact);
	color = vec4(shadow);
}