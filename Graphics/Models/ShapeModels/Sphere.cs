using GraphicsPlayground.Graphics.Models.Generic;
using GraphicsPlayground.Graphics.Models.Mesh;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Models.ShapeModels;

public class Sphere : GenericMesh
{
    public float Radius;
    public uint Stacks;
    public uint Slices;

    public Sphere(GenericModelPart modelPart, float radius, uint stacks, uint slices) : base("Sphere Mesh", modelPart)
    {
        Radius = radius;
        Stacks = stacks;
        Slices = slices;
        Generate(radius, stacks, slices);
    }

    public void Generate(float radius, uint stacks, uint slices)
    {
        // Calculate the number of vertices and indices
        uint vertexCount = (stacks + 1) * (slices + 1);
        uint numIndices = stacks * slices * 6;  // 2 triangles per stack/slice, 3 indices per triangle

        List<Vector3> vertices = new((int)vertexCount);
        List<Vector3> normals = new((int)vertexCount);
        List<Vector2> textureCoords = new((int)vertexCount);

        uint[] indices = new uint[numIndices];
        int indexIndex = 0;

        for (int i = 0; i <= stacks; i++)
        {
            float theta = i * MathHelper.Pi / stacks;
            float sinTheta = (float)Math.Sin(theta);
            float cosTheta = (float)Math.Cos(theta);

            for (int j = 0; j <= slices; j++)
            {
                float phi = j * 2 * MathHelper.Pi / slices;
                float sinPhi = (float)Math.Sin(phi);
                float cosPhi = (float)Math.Cos(phi);

                float x = radius * cosPhi * sinTheta;
                float y = radius * cosTheta;
                float z = radius * sinPhi * sinTheta;

                vertices.Add(new Vector3(x, y, z));

                Vector3 normal = new(x, y, z);
                normals.Add(normal.Normalized());

                textureCoords.Add(new Vector2((float)j / slices, (float)i / stacks));

                if (i < stacks && j < slices)
                {
                    uint currentRow = (uint)(i * (slices + 1));
                    uint nextRow = (uint)((i + 1) * (slices + 1));

                    indices[indexIndex++] = (uint)(currentRow + j);
                    indices[indexIndex++] = (uint)(nextRow + j);
                    indices[indexIndex++] = (uint)(currentRow + j + 1);

                    indices[indexIndex++] = (uint)(currentRow + j + 1);
                    indices[indexIndex++] = (uint)(nextRow + j);
                    indices[indexIndex++] = (uint)(nextRow + j + 1);
                }
            }
        }

        if (vertices.Count != normals.Count || vertices.Count != textureCoords.Count)
            throw new ArgumentException("Position, normal, and texture coordinate lists must have the same length.");

        Vertices = vertices;
        Array.Reverse(indices);
        List<uint> indicesList = new(indices);
        Indices = indicesList;
        TextureCoords = textureCoords;
        Normals = normals;
    }
}
