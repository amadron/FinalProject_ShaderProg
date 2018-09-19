#version 430 core
uniform sampler2D inputTexture;

in vec2 uv;

out vec4 color;
void main()
{
	color = texture(inputTexture, uv);
	//gl_FragColor = vec4(0.5);
}