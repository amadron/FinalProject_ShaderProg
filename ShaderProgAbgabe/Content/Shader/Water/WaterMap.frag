#version 430 core
#include "waterutil.glsl"

struct Wave{
	float amplitude;
	vec2 direction;
	float steepness;
	float wavelength;
	float speed;
};

uniform float time;
layout(std430) buffer WavesBuffer
{
	Wave waves[];
};
uniform int numberOfWaves;

in Data {
	vec3 position;
	vec3 normal;
	vec2 uv;
} inData;

out vec4 color;
out vec4 normal;
void main()
{
	vec3 inpos = inData.position;
	float freq = 5;
	vec3 sumPosVector = vec3(0);
	
	for(int i = 0; i < numberOfWaves; i++)
	{
		Wave tmpwav = waves[i]; 
		sumPosVector += getWavePosition(inData.position, tmpwav.steepness, tmpwav.amplitude, tmpwav.wavelength, tmpwav.direction, tmpwav.speed, time);
	}
	sumPosVector.x += inData.position.x;
	sumPosVector.y += inData.position.y;

	
	
	color = vec4(vec3(sumPosVector.z),1 );
	
}