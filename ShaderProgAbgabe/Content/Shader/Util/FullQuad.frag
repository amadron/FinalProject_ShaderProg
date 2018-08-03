#version 430 core
uniform sampler2D inputTexture;

in vec2 uv;

void main()
{
	gl_FragColor = texture(inputTexture, uv);
	//gl_FragColor = vec4(0.5);
}