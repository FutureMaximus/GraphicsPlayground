using GraphicsPlayground.Graphics.Models.Generic;
using GraphicsPlayground.Graphics.Models.Mesh;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Models.ShapeModels;

public class Torus : GenericMesh
{
    public float Radius;
    public float TubeRadius;
    public uint Sides;
    public uint Rings;

    public Torus(GenericModelPart modelPart, float radius, float tubeRadius, uint sides, uint rings) : base("Torus Mesh" , modelPart)
    {
        Radius = radius;
        TubeRadius = tubeRadius;
        Sides = sides;
        Rings = rings;
        Generate(radius, tubeRadius, sides, rings);
    }
    
    public void Generate(float radius, float tubeRadius, uint sides, uint rings)
    {
        // Calculate the number of vertices and indices
        uint vertexCount = (rings + 1) * (sides + 1);
        uint numIndices = rings * sides * 6;  // 2 triangles per ring/side, 3 indices per triangle

        List<Vector3> vertices = new((int)vertexCount);
        List<Vector3> normals = new((int)vertexCount);
        List<Vector2> textureCoords = new((int)vertexCount);

        uint[] indices = new uint[numIndices];
        int indexIndex = 0;

        for (int i = 0; i <= rings; i++)
        {
            float theta = i * MathHelper.TwoPi / rings;
            float sinTheta = (float)Math.Sin(theta);
            float cosTheta = (float)Math.Cos(theta);

            for (int j = 0; j <= sides; j++)
            {
                float phi = j * MathHelper.TwoPi / sides;
                float sinPhi = (float)Math.Sin(phi);
                float cosPhi = (float)Math.Cos(phi);

                float x = (radius + tubeRadius * cosTheta) * cosPhi;
                float y = tubeRadius * sinTheta;
                float z = (radius + tubeRadius * cosTheta) * sinPhi;

                vertices.Add(new Vector3(x, y, z));

                // Calculate partial derivatives
                float dxdTheta = -sinTheta * cosPhi;
                float dydTheta = cosTheta;
                float dzdTheta = -sinTheta * sinPhi;

                float dxdPhi = -(radius + tubeRadius * cosTheta) * sinPhi;
                float dydPhi = 0;  // torus is rotationally symmetric about y-axis
                float dzdPhi = (radius + tubeRadius * cosTheta) * cosPhi;

                // Compute the normal using the cross product of partial derivatives
                Vector3 normal = new(
                    dydTheta * dzdPhi - dzdTheta * dydPhi,
                    dzdTheta * dxdPhi - dxdTheta * dzdPhi,
                    dxdTheta * dydPhi - dydTheta * dxdPhi
                );

                normals.Add(normal.Normalized());

                textureCoords.Add(new Vector2((float)j / sides, (float)i / rings));

                if (i < rings && j < sides)
                {
                    uint currentRow = (uint)(i * (sides + 1));
                    uint nextRow = (uint)((i + 1) * (sides + 1));

                    indices[indexIndex++] = (uint)(currentRow + j);
                    indices[indexIndex++] = (uint)(nextRow + j);
                    indices[indexIndex++] = (uint)(nextRow + j + 1);

                    indices[indexIndex++] = (uint)(currentRow + j);
                    indices[indexIndex++] = (uint)(nextRow + j + 1);
                    indices[indexIndex++] = (uint)(currentRow + j + 1);
                }
            }
        }

        Vertices = vertices;
        List<uint> indicesList = new(indices);
        Array.Reverse(indices);
        Indices = indicesList;
        TextureCoords = textureCoords;
        Normals = normals;
    }
}
