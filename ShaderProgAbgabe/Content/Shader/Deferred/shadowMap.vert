#version 430 core
uniform mat4 camera;
uniform mat4 lightCamera;

uniform int hasHeightMap;
uniform sampler2D heightSampler;
uniform float heightScaleFactor;
uniform int hasInstances;

in vec3 position;
in vec3 normal;
in vec2 uv;

in vec3 instancePosition;
in vec3 instanceScale;
in vec3 instanceRotation;

out vec4 lightPosition;
out vec4 viewPosition;
out vec2 outuv;

void main()
{
	vec3 npos = position;
	float height = (texture2D(heightSampler, uv).r - 0.5) * hasHeightMap * heightScaleFactor;
	npos += normal * height;
	npos += instancePosition * hasInstances;;
	vec4 lpos = lightCamera * vec4(npos, 1);
	lightPosition = lpos;
	vec4 vpos = camera * vec4(npos, 1);
	viewPosition = vpos;
	gl_Position = vpos;
	outuv = uv;
}