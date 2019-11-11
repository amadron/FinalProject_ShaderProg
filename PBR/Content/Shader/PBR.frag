#version 430 core

uniform vec3 albedo_Color;
uniform float roughness;
uniform float metal;
uniform float ao;

in vec3 pos;
in vec3 fragNormal;
in vec2 fragUV;

out vec4 fragColor;
void main()
{
	fragColor = vec4(fragNormal, 1);
}