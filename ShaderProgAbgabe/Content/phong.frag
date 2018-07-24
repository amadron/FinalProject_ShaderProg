#version 430 core
uniform vec4 ambientColor;
uniform vec3 dirLightDir;
uniform vec4 dirLightCol;
uniform vec3 camPos;
uniform vec4 dirSpecCol;
uniform int specFactor;

in vec3 pos;
in vec3 n;

out vec4 color;

vec4 getAmbient()
{
	return ambientColor;
}

vec4 getDiffuse(vec3 normal)
{
	
	vec3 l = -dirLightDir;
	float lambert = max(0, dot(normal, l));
	return dirLightCol * lambert;

}

vec4 getSpecular(vec3 normal)
{
	vec3 l = -dirLightDir;
	vec3 r = reflect(l, normal) ;
	vec3 v = normalize(pos - camPos);
	float spec = max(0, dot(r, v)); 
	return  dirSpecCol * max(0, pow(spec, specFactor));
}


vec4 getPhongColor()
{
	vec3 normal = normalize(n);
	vec4 ambient = getAmbient();
	vec4 diff =  getDiffuse(normal);
	vec4 spec = getSpecular(normal);
	return diff + spec;
}

void main()
{
	
	color = getPhongColor();
	

}