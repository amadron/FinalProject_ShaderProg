﻿#version 430 core

uniform mat4 camera;
uniform int hasHeightMap;
uniform sampler2D heightSampler;
uniform float heightScaleFactor;

in vec3 position;
in vec3 normal;
in vec2 uv;

out Data {
	vec4 position;
	vec4 transPos;
	vec3 normal;
	flat uint material;
	vec2 uv;
} outdata;

void main()
{
	vec3 npos = position;
	float height = (texture2D(heightSampler, uv).r - 0.5) * hasHeightMap * heightScaleFactor;
	npos += normal * height;
	vec4 transPos = camera * vec4(npos, 1);
	gl_Position = transPos;
	outdata.position = vec4(npos,1);
	outdata.normal = normal;
	outdata.material = uint(uv.x);;
	outdata.uv = uv;
	outdata.transPos = transPos;
}