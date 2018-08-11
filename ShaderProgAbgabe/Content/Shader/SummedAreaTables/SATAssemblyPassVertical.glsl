#version 430 core
uniform sampler2D sourceSampler;

uniform int blockLengthX;
uniform int blockLengthY;
uniform int amountBlockX;
uniform int amountBlockY;

in vec2 uv;

out vec4 color;
void main()
{
	int currX = int (gl_FragCoord.x);
	int currY = int (gl_FragCoord.y);
	float currBlockX =  floor(currX/blockLengthX);
	float currBlockY = floor(currY/blockLengthY);
	float currBlockStartX = currBlockX * blockLengthX;
	float currBlockStartY = currBlockY * blockLengthY;
	int startX = int (currBlockStartX);
	int startY = int (currBlockStartY);
	vec4 currVal = texture(sourceSampler, uv);
	for(int i = 0; i < currBlockY; i++)
	{
		int currPrevBlockY = (i + 1) * blockLengthY;
		int currPrevY = clamp(currPrevBlockY - 1, 0, currY);
		
		currVal += texelFetch(sourceSampler, ivec2(currX, currPrevY),0);
	}
	color = currVal;
	//gl_FragColor = vec4(1);
	//color =vec4(0.1f);
}