using GraphicsPlayground.Graphics.Models.Generic;
using GraphicsPlayground.Graphics.Models.Mesh;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Models.ShapeModels;

// Based on https://en.wikipedia.org/wiki/Superformula
public class SuperFormula : GenericMesh
{
    public SuperFormula(GenericModelPart coreModelPart, float[] n, float[] a, float maxScale) : base("SuperFormula Mesh", coreModelPart)
    {
        GenerateSuperFormulaMesh(n, a, maxScale);
    }

    public void GenerateSuperFormulaMesh(float[] n, float[] a, float maxScale)
    {
        List<Vector3> vertices = new();
        List<Vector3> normals = new();
        List<Vector2> texCoords = new();
        List<uint> indices = new();

        // Effectively the resolution of the mesh
        float step = 0.05f * maxScale;
        int nu = (int)(2 * Math.PI / step);
        int nv = (int)(Math.PI / step);

        for (int i = 0; i < nu; i++)
        {
            for (int j = 0; j < nv; j++)
            {
                float u = (float)(-Math.PI + i * step);
                float v = -(float)(Math.PI / 2) + j * step;

                float raux1 = (float)(Math.Pow(Math.Abs(1 / a[0] * Math.Abs(Math.Cos(n[0] * u / 4))), n[2]) +
                                  Math.Pow(Math.Abs(1 / a[1] * Math.Abs(Math.Sin(n[0] * u / 4))), n[3]));
                float r1 = (float)Math.Pow(Math.Abs(raux1), -1 / n[1]);

                float raux2 = (float)(Math.Pow(Math.Abs(1 / a[0] * Math.Abs(Math.Cos(n[0] * v / 4))), n[2]) +
                                  Math.Pow(Math.Abs(1 / a[1] * Math.Abs(Math.Sin(n[0] * v / 4))), n[3]));
                float r2 = (float)Math.Pow(Math.Abs(raux2), -1 / n[1]);

                float x = r1 * (float)Math.Cos(u) * r2 * (float)Math.Cos(v);
                float y = r1 * (float)Math.Sin(u) * r2 * (float)Math.Cos(v);
                float z = r2 * (float)Math.Sin(v);

                vertices.Add(new Vector3(x, y, z));
                texCoords.Add(new Vector2(u, v));
            }
        }

        // Calculate normals
        for (int i = 0; i < nu; i++)
        {
            for (int j = 0; j < nv; j++)
            {
                // Calculate normals based on vertex positions
                Vector3 normal = CalculateNormal(vertices, i, j, nu, nv);
                normals.Add(normal);
            }
        }

        // Calculate indices
        for (int i = 0; i < nu - 1; i++)
        {
            for (int j = 0; j < nv - 1; j++)
            {
                uint currentIndex = (uint)(i * nv + j);
                uint nextIndex = (uint)(currentIndex + nv);

                // Define triangles
                indices.Add(currentIndex);
                indices.Add(currentIndex + 1);
                indices.Add(nextIndex);

                indices.Add(nextIndex);
                indices.Add(currentIndex + 1);
                indices.Add(nextIndex + 1);
            }
        }

        Vertices = vertices;
        Normals = normals;
        TextureCoords = texCoords;
        Indices = indices;
    }

    private static Vector3 CalculateNormal(List<Vector3> vertices, int i, int j, int nu, int nv)
    {
        int currentIndex = i * nv + j;
        if (currentIndex < 0 || currentIndex >= vertices.Count)
        {
            // ProgramHandle the case where the current index is out of bounds
            return Vector3.UnitY;
        }

        Vector3 currentVertex = vertices[currentIndex];

        int nextIndexRight = (i < nu - 1) ? (currentIndex + 1) : currentIndex;
        int nextIndexDown = (j < nv - 1) ? (currentIndex + nv) : currentIndex;

        Vector3 rightVertex = Vector3.Zero;
        Vector3 downVertex = Vector3.Zero;

        // Fallback if the next indices are out of bounds
        if (nextIndexRight >= 0 && nextIndexRight < vertices.Count)
        {
            rightVertex = vertices[nextIndexRight];
        }

        if (nextIndexDown >= 0 && nextIndexDown < vertices.Count)
        {
            downVertex = vertices[nextIndexDown];
        }

        if (rightVertex == Vector3.Zero || downVertex == Vector3.Zero)
        {
            // ProgramHandle the case where the next indices are out of bounds or fallback values are not set
            return Vector3.UnitY;
        }

        Vector3 rightEdge = rightVertex - currentVertex;
        Vector3 downEdge = downVertex - currentVertex;

        Vector3 normal = Vector3.Cross(downEdge, rightEdge).Normalized();

        return normal;
    }
}
