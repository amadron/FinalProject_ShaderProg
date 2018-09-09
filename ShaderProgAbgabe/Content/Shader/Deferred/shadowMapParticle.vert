#version 430 core
#include "deferredutil.glsl"

in vec3 position;
in vec3 normal;
in vec2 uv;

in vec3 instancePosition;
in vec3 instanceScale;
in vec3 instanceRotation;

uniform mat4 camera;
uniform mat4 cameraViewMatrix;
uniform mat4 lightCamera;
uniform mat4 lightViewMatrix;

out vec4 lightPosition;
out vec4 viewPosition;
out vec2 outuv;

void main()
{
	vec3 upCam = getCameraUpVector(cameraViewMatrix);
	vec3 rightCam = getCameraRightVector(cameraViewMatrix);
	vec3 upLight = getCameraUpVector(lightViewMatrix);
	vec3 rightLight = getCameraUpVector(lightViewMatrix);
	vec3 npos = getBillboardPosition(position, instanceScale, upCam, rightCam);
	//vec3 npos = position * instanceScale;
	npos += instancePosition;
	//vec4 lpos = lightCamera * vec4(npos, 1);
	vec3 lpos = getBillboardPosition(position, instanceScale, upLight, rightLight);
	lpos += instancePosition;
	lightPosition = lightCamera *  vec4(lpos, 1);
	vec4 vpos = camera * vec4(npos, 1);
	viewPosition = vpos;
	gl_Position = vpos;
	outuv = uv;
}