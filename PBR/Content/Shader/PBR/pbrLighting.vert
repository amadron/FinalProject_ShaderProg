#version 430 core

in vec3 position;
in vec3 normal;
in vec2 uv;
in vec3 tangent;
in vec3 biTangent;

uniform mat4 modelMatrix;
uniform mat4 cameraMatrix;

out Fragment_Data
{
	vec3 worldPos;
	vec3 Normal;
	vec2 UV;
	mat3 tbn;
};

void main()
{
	//pos = position;
	Normal = normal;
	//UV = uv;
	UV = uv;
	//worldPos = position;
	worldPos = (modelMatrix * vec4(position,1)).xyz;
	vec4 viewPos = cameraMatrix * vec4(worldPos,1);
	gl_Position = viewPos;
	tbn = mat3(tangent, biTangent, normal);
}