#version 430 core
uniform mat4 lightCamera;

uniform int hasHeightMap;
uniform sampler2D heightSampler;
uniform float heightScaleFactor;

in vec3 position;
in vec3 normal;
in vec2 uv;

out vec4 lightPosition;
out vec4 viewPosition;

void main()
{
	vec3 npos = position;
	float height = (texture2D(heightSampler, uv).r - 0.5) * hasHeightMap * heightScaleFactor;
	npos += normal * height;
	vec4 lpos = lightCamera * vec4(npos,1);
	lightPosition = lpos;
	gl_Position = lpos;
}