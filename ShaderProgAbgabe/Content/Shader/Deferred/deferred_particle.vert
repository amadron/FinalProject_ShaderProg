#version 430 core
#include "deferredutil.glsl"

uniform vec3 cameraPosition;
uniform vec3 cameraDirection;
uniform mat4 camera;
uniform mat4 viewMatrix;


in vec3 position;
in vec3 normal;
in vec2 uv;

in vec3 instancePosition;
in vec3 instanceScale;
in vec3 instanceRotation;
in vec3 instanceColor;

out Data {
	vec4 position;
	vec4 transPos;
	vec3 normal;
	flat uint material;
	vec2 uv;
	vec4 color;
} outdata;

void main()
{
	vec3 up = getCameraUpVector(viewMatrix);
	vec3 right = getCameraRightVector(viewMatrix);
	//vec3 npos = position * instanceScale;
	vec3 npos = getBillboardPosition(position, instanceScale, up, right);
	//vec3 npos = up * position.y * instanceScale.y  + right * position.x * instanceScale.x;
	npos += instancePosition;
	vec4 transPos = camera * vec4(npos, 1);
	gl_Position = transPos;
	outdata.position = vec4(npos,1);
	outdata.normal = normalize(cameraPosition - npos);
	outdata.material = uint(uv.x);;
	outdata.uv = uv;
	outdata.transPos = transPos;
	outdata.color = vec4(instanceColor,1);
}