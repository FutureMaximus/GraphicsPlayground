using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace GraphicsPlayground.Graphics.Models.Generic;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct GenericVertexData(Vector3 position, Vector3 normal, Vector2 texCoords)
{
    public Vector3 Position = position;
    public Vector3 Normal = normal;
    public Vector2 TextureCoords = texCoords;
    /*public int[] BoneIDs = boneIds;
    public float[] Weights = weights;*/
}
