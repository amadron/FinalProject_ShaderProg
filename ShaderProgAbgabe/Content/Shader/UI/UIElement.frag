#version 430 core
uniform sampler2D inputTexture;
uniform vec4 elementColor;
uniform int hasTexture;

in vec2 uv;

out vec4 color;
void main()
{
	vec4 tex = texture(inputTexture, uv);
	tex += (1 - hasTexture) * vec4(1);
	color = tex * elementColor;
}