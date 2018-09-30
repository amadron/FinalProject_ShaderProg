#version 430 core
#include "deferredutil.glsl"

in vec3 position;
in vec3 normal;
in vec2 uv;

in vec3 instancePosition;
in vec3 instanceScale;
in vec3 instanceRotation;

uniform mat4 lightCamera;
uniform mat4 lightViewMatrix;

out vec4 lightPosition;
out vec4 viewPosition;
out vec2 passUv;

void main()
{
	vec3 up = getCameraUpVector(lightViewMatrix);
	vec3 right = getCameraRightVector(lightViewMatrix);
	vec3 npos = getBillboardPosition(position, instanceScale, up, right);
	npos += instancePosition;
	vec4 lpos = lightCamera * vec4(npos,1);
	lightPosition = lpos;
	gl_Position = lpos;
	passUv = uv;
}