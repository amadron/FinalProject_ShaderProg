#version 430 core
#include "deferredutil.glsl"

uniform mat4 camera;
uniform int hasHeightMap;
uniform sampler2D heightSampler;
uniform float heightScaleFactor;
uniform int hasInstances;

in vec3 position;
in vec3 normal;
in vec2 uv;

in vec3 instancePosition;
in vec4 instanceRotation;
in vec3 instanceScale;

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
	vec3 npos = position * (1 - hasInstances) + rotateByQuaternion(vec4(position,0), instanceRotation) * hasInstances;
	float height = (texture2D(heightSampler, uv).r - 0.5) * hasHeightMap * heightScaleFactor;
	npos += normal * height;
	npos +=  instancePosition * hasInstances;
	vec4 transPos = camera * vec4(npos, 1);
	gl_Position = transPos;
	vec3 heightNorm = getHeightMapNormal(heightSampler, uv, heightScaleFactor) * hasHeightMap;
	outdata.position = vec4(npos,1);
	outdata.normal = normal * (1 - hasHeightMap) + heightNorm;
	outdata.material = uint(uv.x);;
	outdata.uv = uv;
	outdata.transPos = transPos;
	outdata.color = instanceRotation;
}