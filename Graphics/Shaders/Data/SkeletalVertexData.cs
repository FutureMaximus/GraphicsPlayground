using GraphicsPlayground.Graphics.Shaders.Data;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace GraphicsPlayground.Graphics.Shader.Data;

[StructLayout(LayoutKind.Sequential)]
public struct SkeletalVertexData(
    Vector3 position,
    Vector3 normal,
    Vector2 texCoords,
    BoneWeight boneWeight1,
    BoneWeight boneWeight2,
    BoneWeight boneWeight3,
    BoneWeight boneWeight4)
{
    public Vector3 Position = position;
    public Vector3 Normal = normal;
    public Vector2 TextureCoords = texCoords;
    public BoneWeight BoneWeight1 = boneWeight1;
    public BoneWeight BoneWeight2 = boneWeight2;
    public BoneWeight BoneWeight3 = boneWeight3;
    public BoneWeight BoneWeight4 = boneWeight4;
}
