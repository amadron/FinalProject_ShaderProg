#version 430 core
#include "util.glsl"

uniform vec4 ambientColor;
uniform vec3 camPos;
uniform vec3 camDir;

uniform vec3 dirLightDir;
uniform vec4 dirLightCol;
uniform vec4 dirSpecCol;
uniform float dirIntensity;
uniform float dirSpecIntensity;
uniform float specFactor;

uniform sampler2D positionSampler;
uniform sampler2D albedoSampler;
uniform sampler2D normalSampler;
uniform sampler2D pointLightSampler;
uniform sampler2D shadowSampler;

in vec2 uv;

out vec4 color;


void main()
{
	vec3 position = texture2D(positionSampler, uv).xyz;
	vec4 albedo = texture2D(albedoSampler, uv);
	vec3 normal = texture2D(normalSampler, uv).rgb;
	vec4 shadows = texture2D(shadowSampler, uv);
	vec3 ambient = ambientColor.rgb;
	vec4 diffuse = getDiffuse(dirLightDir, normal, dirLightCol, albedo, dirIntensity);
	vec3 viewDir = normalize(position - camPos);
	//viewDir = -normalize(camDir);
	vec4 specular = getSpecular(viewDir, normal, dirLightDir, dirSpecCol, specFactor, dirSpecIntensity);
	vec4 col = diffuse + specular + vec4(ambient, 1);
	vec4 plightColor = texture(pointLightSampler, uv);
	color = plightColor + (col * shadows);
}