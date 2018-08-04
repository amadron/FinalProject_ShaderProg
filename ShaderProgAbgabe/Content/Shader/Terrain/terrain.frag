#version 430 core

uniform sampler2D material;

in vec2 intuv;

in Data {
	vec4 position;
	vec3 normal;
	vec2 uv;
} inData;

out vec4 position;
out vec4 albedo;
out vec4 normal;

void main()
{
	//vec4 mat = texture(material, inData.uv);
	albedo = vec4(0.5);
}