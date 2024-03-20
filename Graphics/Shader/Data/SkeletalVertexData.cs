using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace GraphicsPlayground.Graphics.Shader.Data;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct SkeletalVertexData(Vector3 position, Vector3 normal, Vector2 texCoords, int[] boneIds, float[] weights)
{
    public Vector3 Position = position;
    public Vector3 Normal = normal;
    public Vector2 TextureCoords = texCoords;
    public int[] BoneIDs = boneIds;
    public float[] Weights = weights;
}
