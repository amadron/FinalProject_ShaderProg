#version 430 core

uniform sampler2D sourceSampler;

uniform int width;
uniform int height;

uniform int halfKernelY;
uniform int halfKernelX;

out vec4 color;
void main()
{
	int currX = int (gl_FragCoord.x);
	int currY = int (gl_FragCoord.y);
	int lX = clamp(currX - halfKernelX, 0, width);
	int rX = clamp(currX + halfKernelX, 0, width);
	int uY = clamp(currY - halfKernelY, 0, height);
	int dY = clamp(currY + halfKernelY, 0, height);
	vec4 ul = texelFetch(sourceSampler, ivec2(lX, uY), 0);
	vec4 ur = texelFetch(sourceSampler, ivec2(rX, uY), 0);
	vec4 lr = texelFetch(sourceSampler, ivec2(rX, dY), 0);
	vec4 ll = texelFetch(sourceSampler, ivec2(lX, dY), 0);
	color = (lr - ur - ll + ul) / (halfKernelX * halfKernelY * 4);
}