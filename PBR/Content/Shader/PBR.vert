﻿#version 430 core

in vec3 position;
in vec3 normal;
in vec2 uv;
uniform mat4 cameraMatrix;

out vec3 pos;
out vec3 Normal;
out vec2 UV;

void main()
{
	pos = (cameraMatrix * vec4(position,1)).xyz;
	//pos = position;
	Normal = normal;
	UV = uv;

	gl_Position = vec4(pos, 1);
}