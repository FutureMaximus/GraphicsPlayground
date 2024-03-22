#version 430 core

in VS_OUT {
	vec3  FragPos;
	vec3  Normal;
} fs_in;

out vec4 FragColor;

void main()
{
	FragColor = vec4(vec3(1), 1.0);
}