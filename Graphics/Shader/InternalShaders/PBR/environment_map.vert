#version 430 core
layout (location = 0) in vec3 aPosition;

out vec3 WorldPos;

uniform mat4 projection;
uniform mat4 view;

void main()
{
    WorldPos = aPosition;
    gl_Position = projection * view * vec4(WorldPos, 1.0);
}