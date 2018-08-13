﻿#version 430 core

uniform vec4 ambientColor;
uniform vec3 camPos;

uniform vec3 dirLightDir;
uniform vec4 dirLightCol;
uniform vec4 dirSpecCol;
uniform float dirIntensity;
uniform float dirSpecIntensity;
uniform int specFactor;

uniform sampler2D positionSampler;
uniform sampler2D albedoSampler;
uniform sampler2D normalSampler;
uniform sampler2D pointLightSampler;
uniform sampler2D shadowSampler;

in vec2 uv;

out vec4 color;

vec4 getDiffuse(vec3 lightDirection, vec3 normal, vec4 lightColor, vec4 albedo)
{
	
	vec3 l = -normalize(lightDirection);
	float lambert = max(0, dot(normal, l));
	return lightColor * lambert * albedo;

}

vec4 getSpecular(vec3 viewDir, vec3 normal, vec3 lightDirection, vec4 SpecularColor,int specularFactor)
{
	vec3 l = -normalize(lightDirection);
	vec3 r = reflect(l, normal) ;
	vec3 v = -viewDir;
	float spec = max(0, dot(r, v)); 
	return  SpecularColor * max(0, pow(spec, specularFactor)) * dirSpecIntensity;
}

void main()
{
	vec3 position = texture2D(positionSampler, uv).xyz;
	vec4 albedo = texture2D(albedoSampler, uv);
	vec3 normal = texture2D(normalSampler, uv).rgb;
	vec4 shadows = texture2D(shadowSampler, uv);
	vec3 ambient = ambientColor.rgb;
	vec4 diffuse = getDiffuse(dirLightDir, normal, dirLightCol, albedo) * dirIntensity;
	vec3 viewDir = normalize(position - camPos);
	vec4 specular = getSpecular(viewDir, normal, dirLightDir, dirSpecCol, specFactor);
	vec4 col = diffuse + specular + vec4(ambient,1);
	vec4 plightColor = texture(pointLightSampler, uv);
	color = plightColor + (col * shadows * shadows);
	//gl_FragColor = shadows;
}