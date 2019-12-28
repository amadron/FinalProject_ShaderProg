#version 330 core

in vec3 position;

out vec2 texCoord;

vec3 positions[4] = vec3[4](
    vec3(-1, -1, 0),
    vec3(1, -1, 0),
    vec3(1, 1, 0),
    vec3(-1, 1, 0)
);

vec2 uvs[4] = vec2[4](
    vec2(0,0),
    vec2(1,0),
    vec2(1,1),
    vec2(0,1)
);

void main()
{
    gl_Position =  vec4(positions[gl_VertexID],1);
    texCoord = uvs[gl_VertexID];
}