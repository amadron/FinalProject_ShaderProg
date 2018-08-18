#version 430 core
#include "util.glsl"

uniform sampler2D positionSampler;
uniform sampler2D albedoSampler;
uniform sampler2D normalSampler;

uniform mat4 camera;
uniform vec3 cameraPosition;

in vec3 position;
in vec3 normal;

in Data 
{
	vec4 position;
	vec3 normal;
	vec4 lightColor;
	vec3 lightPosition;
	float radius;
	float intensity;
	vec4 specularColor;
	float specFactor;
	float specIntensity;
} inData;

out vec4 color;



void main()
{
	vec3 pos = inData.position.xyz / inData.position.w;
	vec2 uv = pos.xy * 0.5f + 0.5f;
	vec4 scnAlbedo = texture(albedoSampler, uv);
	vec3 scnNormal = texture(normalSampler, uv).xyz;
	vec3 scnPosition = texture(positionSampler, uv).xyz;
	vec3 lpos = inData.lightPosition;
	vec3 ldir = scnPosition - lpos;
	//Taken anuttation from Example
	float dist = length(ldir);
	ldir = normalize(ldir);
	scnNormal = normalize(scnNormal);
	//scnAlbedo = clamp(scnAlbedo, 0,1);
	float intensity = inData.intensity;
	vec4 diffuse = getDiffuse(ldir, scnNormal, inData.lightColor, scnAlbedo, intensity);
	//Check if ScenePosition is within range of lightsource

	float falloff = clamp(inData.radius - dist, 0, inData.radius);
	//Specular
	vec3 viewDir = scnPosition - cameraPosition;
	vec4 specular = getSpecular(viewDir, scnNormal, ldir, inData.specularColor, inData.specFactor, inData.specIntensity);
	//color = diffuse;
	vec4 result = falloff * diffuse;
	color = result;
}