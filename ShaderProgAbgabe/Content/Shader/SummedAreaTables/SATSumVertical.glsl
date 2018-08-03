#version 430 core
uniform sampler2D sourceSampler;

uniform int blockLengthX;
uniform int blockLengthY;

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
		float currVal = texture(sourceSampler, uv).r;
		for(int j = startY; j < currY; j++)
		{
			currVal += texelFetch(sourceSampler, ivec2(currX, j), 0).r;
		}
		color = vec4(vec3(currVal), 1);
}