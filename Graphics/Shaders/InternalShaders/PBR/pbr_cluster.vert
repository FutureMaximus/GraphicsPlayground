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

#ifdef SKELETAL_MESH
const int MAX_BONES = 100;
const int MAX_BONE_INFLUENCE = 4;
uniform mat4 finalBonesMatrices[MAX_BONES];
#endif

void main()
{
	vec3 position;
	#ifdef SKELETAL_MESH
	vec4 totalPosition = vec4(0.0f);
    for(int i = 0 ; i < MAX_BONE_INFLUENCE; i++)
    {
        if(boneIds[i] == -1) 
            continue;
        if(boneIds[i] >= MAX_BONES) 
        {
            totalPosition = vec4(pos, 1.0f);
            break;
        }
        vec4 localPosition = vec4(aPosition, 1.0f) * finalBonesMatrices[boneIds[i]];
        totalPosition += localPosition * weights[i];
        vec3 localNormal = norm * mat3(finalBonesMatrices[boneIds[i]]);
	}
	position = totalPosition.xyz;
	#else
	position = aPosition;
	#endif
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