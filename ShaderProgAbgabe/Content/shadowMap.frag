#version 430 core
uniform sampler2D lightViewSampler;

in vec4 lightPosition;
in vec4 viewPosition;

out vec4 color;

void main()
{
	vec3 lpos = lightPosition.xyz/lightPosition.w;
	vec3 vpos = viewPosition.xyz/viewPosition.w;
	float vdepth = lpos.z;
	vec4 smapPos = texture(lightViewSampler, lpos.xy * 0.5 + 0.5);
	float lightViewDepth = smapPos.r + 0.00001;
	vec4 res = vec4(0,0,0,0);
	//zEye < zLight -> Fragment is in schadow
	if(lightViewDepth < vdepth)
	{
		res = vec4(1,1,1,1);
	}
	color = res;
}