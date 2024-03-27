#version 430 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;
layout (location = 3) in vec3 aTangent;

layout (std140, binding = 0) uniform ProjView
{
	mat4 projection;
	mat4 view;
	vec3 viewPos;
};

out VS_OUT {
	vec3  FragPos;
	vec2  TexCoords;
	vec3  Normal;
	mat3  TBN;
} vs_out;

uniform mat4 model;
uniform mat3 normalMatrix;
uniform bool hasTangents;

void main()
{
	gl_Position = vec4(aPosition, 1.0) * model * view * projection;
	vs_out.FragPos = vec3(vec4(aPosition, 1.0) * model);
	vec3 normal = normalize(aNormal * normalMatrix);
	vs_out.Normal = normal;
	vs_out.TexCoords = aTexCoords;
	if (hasTangents) {
		vec3 T = normalize(aTangent * normalMatrix);
		vec3 N = normal;
		T = normalize(T - dot(T, N) * N);
		vec3 B = normalize(cross(N, T));
		mat3 TBN = transpose(mat3(T, B, N));
		vs_out.TBN = TBN;
	}
}