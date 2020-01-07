#version 330 core
in vec3 position;
in vec2 uv;
in vec3 normal;

out vec2 TexCoords;
out vec3 WorldPos;
out vec3 Normal;

uniform mat4 modelMatrix;
uniform mat4 cameraMatrix;

void main()
{
    TexCoords = uv;
    WorldPos = vec3(modelMatrix * vec4(position, 1.0));
    Normal = mat3(modelMatrix) * normal;   

    gl_Position =  cameraMatrix * vec4(WorldPos, 1.0);
}