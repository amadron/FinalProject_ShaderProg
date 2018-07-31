#version 430 core

in vec4 lightPosition;

out vec4 color;
void main()
{
	
	color = vec4(lightPosition.z / lightPosition.w);
}