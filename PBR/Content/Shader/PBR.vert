#version 430 core

in vec3 position;
in vec3 normal;
in vec2 uv;
uniform mat4 cameraMatrix;

out vec3 pos;
out vec3 fragNormal;
out vec2 fragUV;

void main()
{
	pos = (cameraMatrix * vec4(position,1)).xyz;
	//pos = position;
	fragNormal = normal;
	fragUV = uv;

	gl_Position = vec4(pos, 1);
}