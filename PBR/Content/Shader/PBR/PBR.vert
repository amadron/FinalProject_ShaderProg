#version 430 core


in vec3 position;
in vec3 normal;
in vec2 uv;
uniform mat4 modelMatrix;
uniform mat4 cameraMatrix;
uniform vec3 albedo;

out vec3 worldPos;
out vec3 Normal;
out vec2 UV;

void main()
{
	//pos = position;
	Normal = normal;
	UV = uv;
	//worldPos = position;
	worldPos = (modelMatrix * vec4(position,1)).xyz;
	vec4 viewPos = cameraMatrix * vec4(worldPos,1);
	gl_Position = viewPos;
}