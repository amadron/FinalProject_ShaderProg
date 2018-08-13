#version 430 core
uniform int hasAlbedo;
uniform sampler2D albedoSampler;
uniform int hasNormalMap;
uniform sampler2D normalSampler;

in Data {
	vec4 position;
	vec4 transPos;
	vec3 normal;
	flat uint material;
	vec2 uv;
} inData;

out vec4 position;
out vec4 albedo;
out vec4 normal;

void main()
{
	const vec3 materials[] = { vec3(1), vec3(1, 0, 0), vec3(0, 1, 0), vec3(0, 1, 1) };
	//vec3 pos = inData.position;
	position = inData.position / inData.position.w;
	//position = inData.transPos;
	albedo = vec4(materials[inData.material],1) * (1 - hasAlbedo) +	texture(albedoSampler, inData.uv) * hasAlbedo;
	vec3 n = inData.normal * (1 - hasNormalMap) + -texture(normalSampler, inData.uv).rgb * hasNormalMap;
	n = normalize(n);
	normal = vec4(n,1);
}