#version 430 core

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

vec4 getDiffuse(vec3 lightDirection, vec3 normal, vec4 lightColor)
{
	
	vec3 l = -lightDirection;
	float lambert = max(0, dot(l, normal));
	return lightColor * lambert;

}

vec4 getSpecular(vec3 lightDirection, vec3 normal, vec4 specularColor, vec3 viewDirection, float specFactor, float intensity)
{
	vec3 l = -normalize(lightDirection);
	vec3 r = reflect(l, normal) ;
	vec3 v = normalize(viewDirection);
	float spec = max(0, dot(r, v)); 
	return  specularColor * max(0, pow(spec, specFactor)) * intensity;
}

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
	vec4 diffuse = getDiffuse(ldir, scnNormal, inData.lightColor);
	float intensity = inData.intensity;
	//Check if ScenePosition is within range of lightsource
	float falloff = clamp(inData.radius - dist, 0, inData.radius);
	//Specular
	vec3 viewDir = scnPosition - cameraPosition;
	vec4 specular = getSpecular(ldir, scnNormal, inData.specularColor, viewDir, inData.specFactor, inData.specIntensity);
	//color = diffuse;
	//color = vec4(0);
	color = specular * falloff + diffuse * intensity * falloff;
}