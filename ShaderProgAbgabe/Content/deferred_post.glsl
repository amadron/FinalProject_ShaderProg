#version 430 core

uniform vec4 ambientColor;
uniform vec3 camPos;

uniform vec3 dirLightDir;
uniform vec4 dirLightCol;
uniform vec4 dirSpecCol;

uniform sampler2D albedoSampler;
uniform sampler2D normalSampler;

in vec2 uv;

vec4 getDiffuse(vec3 lightDirection, vec3 normal, vec4 lightColor)
{
	
	vec3 l = -lightDirection;
	float lambert = max(0, dot(normal, l));
	return lightColor * lambert;

}

vec4 getSpecular(vec3 position, vec3 normal, vec3 lightDirection, vec3 cameraPosition, vec4 SpecularColor,int specularFactor)
{
	vec3 l = -lightDirection;
	vec3 r = reflect(l, normal) ;
	vec3 v = normalize(position - cameraPosition);
	float spec = max(0, dot(r, v)); 
	return  SpecularColor * max(0, pow(spec, specularFactor));
}

void main()
{
	vec4 albedo = texture2D(albedoSampler, uv);
	vec3 normal = texture2D(normalSampler, uv).rgb;
	vec4 ambient = ambientColor;
	vec4 diffuse = getDiffuse(dirLightDir, normal, dirLightCol) * albedo;
	vec4 color = ambient + diffuse;
	gl_FragColor = color;
}