#version 430 core

uniform sampler2D positionSampler;
uniform sampler2D albedoSampler;
uniform sampler2D normalSampler;


uniform mat4 camera;
in vec3 position;
in vec3 normal;

in Data 
{
	vec4 position;
	vec3 normal;
	vec4 lightColor;
	vec3 lightPosition;
	float lightStrength;
} inData;

out vec4 color;

vec4 getDiffuse(vec3 lightDirection, vec3 normal, vec4 lightColor)
{
	
	vec3 l = -lightDirection;
	float lambert = max(0, dot(normal, l));
	return lightColor * lambert;

}

void main()
{
	vec3 pos = inData.position.xyz / inData.position.w;
	vec2 uv = pos.xy * 0.5f + 0.5f;
	vec4 scnAlbedo = texture(albedoSampler, uv);
	vec3 scnNormal = texture(normalSampler, uv).xyz;
	vec3 scnPosition = texture(positionSampler, uv).xyz;
	vec3 lpos = inData.lightPosition;
	vec3 ldir = normalize(lpos - scnPosition);
	vec4 diffuse = getDiffuse(ldir, scnNormal, inData.lightColor);
	float intensity = inData.lightStrength;
	color = diffuse;
}