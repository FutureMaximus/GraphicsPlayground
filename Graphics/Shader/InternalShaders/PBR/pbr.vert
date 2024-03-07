#version 430 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;
layout (location = 3) in vec3 aTangent;
//layout (location = 3) in ivec4 aBoneIds;
//layout (location = 4) in vec4 aWeights;
//layout (location = 5) in vec3 aTangent;

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
	vec3  ViewPos;
	mat3  TBN;
} vs_out;

const int MAX_BONES = 100;
const int MAX_BONES_INFLUENCE = 4;
uniform mat4 finalBonesMatrices[MAX_BONES];

uniform mat4 model;
uniform mat3 normalMatrix;
uniform bool hasTangents;

void main()
{
	vec4 totalPosition = vec4(0.0);
	//for (int i = 0; i < MAX_BONES_INFLUENCE; i++)
	//{
	//	if (aBoneIds[i] == -1)
	//		continue;
	//	if (aBoneIds[i] >= MAX_BONES)
	//	{
	//		totalPosition = vec4(aPosition, 1.0);
	//		break;
	//	}
	//	vec4 localPosition = vec4(aPosition, 1.0) * finalBonesMatrices[aBoneIds[i]];
	//	totalPosition += localPosition * aWeights[i];
	//	vec3 localNormal = aNormal * mat3(finalBonesMatrices[aBoneIds[i]]);
	//}

	gl_Position = projection * view * model * vec4(aPosition, 1.0); // TODO: Use totalPosition
	vs_out.FragPos = vec3(model * vec4(aPosition, 1.0));
	vec3 normal = normalize(normalMatrix * aNormal);
	vs_out.Normal = normal;
	vs_out.TexCoords = aTexCoords;
	vs_out.ViewPos = viewPos;
	if (hasTangents) {
		vec3 T = normalize(normalMatrix * aTangent);
		vec3 N = normal;
		T = normalize(T - dot(T, N) * N);
		vec3 B = normalize(cross(N, T));
		mat3 TBN = transpose(mat3(T, B, N));
		vs_out.TBN = TBN;
	}
}