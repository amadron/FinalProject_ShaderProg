#version 430 core

in Data {
	vec4 position;
	vec3 normal;
	flat uint material;
} inData;

out vec4 position;
out vec4 albedo;
out vec4 normal;

void main()
{
	const vec3 materials[] = { vec3(1), vec3(1, 0, 0), vec3(0, 1, 0), vec3(0, 1, 1) };
	//vec3 pos = inData.position;
	position = inData.position / inData.position.w;
	albedo = vec4(materials[inData.material],1);
	vec3 n = normalize(inData.normal);
	normal = vec4(n,1);
}