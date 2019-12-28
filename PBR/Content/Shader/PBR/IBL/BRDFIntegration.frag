#version 330 core
#include "IBLUtil.glsl"

out vec2 fragColor;

in vec2 texCoord;

void main()
{
 vec2 integratedBRDF = IntegrateBRDF(texCoord.x, texCoord.y);
 fragColor = integratedBRDF;
 //fragColor = vec2(texCoord);
}