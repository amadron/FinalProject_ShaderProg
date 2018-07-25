﻿#version 430 core

in Data {
	vec3 position;
	vec3 normal;
	flat uint material;
} inData;

out vec4 albedo;
out vec4 normal;

void main()
{
	const vec3 materials[] = { vec3(1), vec3(1, 0, 0), vec3(0, 1, 0), vec3(0, 1, 1) };
	albedo = vec4(materials[inData.material],1);
	normal = vec4(inData.normal,1);
}