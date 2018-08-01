#version 430 core

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

vec4 getDiffuse(vec3 lightDirection, vec3 normal, vec4 lightColor)
{
	
	vec3 l = -lightDirection;
	float lambert = max(0, dot(normal, l));
	return lightColor * lambert;

}

vec4 getSpecular(vec3 viewDir, vec3 normal, vec3 lightDirection, vec4 SpecularColor,int specularFactor)
{
	vec3 l = -lightDirection;
	vec3 r = reflect(l, normal) ;
	vec3 v = viewDir;
	float spec = max(0, dot(r, v)); 
	return  SpecularColor * max(0, pow(spec, specularFactor));
}

void main()
{
	vec3 position = texture2D(positionSampler, uv).xyz;
	vec3 albedo = texture2D(albedoSampler, uv).rgb;
	vec3 normal = texture2D(normalSampler, uv).rgb;
	vec4 shadows = texture2D(shadowSampler, uv);
	vec3 ambient = ambientColor.rgb;
	vec3 diffuse = getDiffuse(dirLightDir, normal, dirLightCol) * albedo * dirIntensity;
	//vec3 diffuse = max(0, dot(normal, -dirLightDir)) * albedo * dirLightCol;
	vec3 viewDir = normalize(position - camPos);
	vec4 specular = getSpecular(viewDir, normal, dirLightDir, dirSpecCol, specFactor) * dirSpecIntensity;
	vec4 color = vec4(diffuse + specular + ambient);
	vec4 plightColor = texture(pointLightSampler, uv);
	gl_FragColor = color * shadows + plightColor;
	//gl_FragColor = shadows;
}