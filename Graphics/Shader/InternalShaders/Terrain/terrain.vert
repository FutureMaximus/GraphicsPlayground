#version 430 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;

layout (std140, binding = 0) uniform ProjView
{
	mat4 projection;
	mat4 view;
	vec3 viewPos;
};

uniform mat4 model;
uniform mat3 normalMatrix;

out VS_OUT {
	vec3  FragPos;
	vec3  Normal;
} vs_out;

void main()
{
    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
	vs_out.FragPos = vec3(vec4(aPosition, 1.0) * model);
	vs_out.Normal = normalize(aNormal * normalMatrix);
}