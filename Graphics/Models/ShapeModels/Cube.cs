using GraphicsPlayground.Graphics.Models.Generic;

namespace GraphicsPlayground.Graphics.Models.ShapeModels;

public class Cube : GenericMesh
{
    public Cube(GenericModelPart modelPart) : base("Cube Mesh", modelPart)
    {
        Generate();
    }

    public void Generate()
    {
        Vertices = GeometryHelper.ListOfVector3FromFloatArray(_vertices);
        TextureCoords = GeometryHelper.ListOfVector2FromFloatArray(_texCoords);
        Indices = new List<uint>(_indices);
        Normals = GeometryHelper.ListOfVector3FromFloatArray(_normals);
    }

    private readonly float[] _vertices = new float[]
    {
        // Front face
        -1, -1, 1,
        1, -1, 1,
        1, 1, 1,
        -1, 1, 1,

        // Back face
        -1, -1, -1,
        -1, 1, -1,
        1, 1, -1,
        1, -1, -1,

        // Top face
        -1, 1, -1,
        -1, 1, 1,
        1, 1, 1,
        1, 1, -1,

        // Bottom face
        -1, -1, -1,
        1, -1, -1,
        1, -1, 1,
        -1, -1, 1,

        // Right face
        1, -1, -1,
        1, 1, -1,
        1, 1, 1,
        1, -1, 1,

        // Left face
        -1, -1, -1,
        -1, -1, 1,
        -1, 1, 1,
        -1, 1, -1
    };

    private readonly float[] _normals = new float[]
    {
        // Front face
        0, 0, 1,
        0, 0, 1,
        0, 0, 1,
        0, 0, 1,

        // Back face
        0, 0, -1,
        0, 0, -1,
        0, 0, -1,
        0, 0, -1,

        // Top face
        0, 1, 0,
        0, 1, 0,
        0, 1, 0,
        0, 1, 0,

        // Bottom face
        0, -1, 0,
        0, -1, 0,
        0, -1, 0,
        0, -1, 0,

        // Right face
        1, 0, 0,
        1, 0, 0,
        1, 0, 0,
        1, 0, 0,

        // Left face
        -1, 0, 0,
        -1, 0, 0,
        -1, 0, 0,
        -1, 0, 0
    };

    private readonly float[] _texCoords = new float[]
    {
        // Front face
        0, 0,
        1, 0,
        1, 1,
        0, 1,

        // Back face
        1, 0,
        1, 1,
        0, 1,
        0, 0,

        // Top face
        0, 1,
        0, 0,
        1, 0,
        1, 1,

        // Bottom face
        1, 1,
        0, 1,
        0, 0,
        1, 0,

        // Right face
        1, 0,
        1, 1,
        0, 1,
        0, 0,

        // Left face
        0, 0,
        1, 0,
        1, 1,
        0, 1
    };

    private readonly uint[] _indices = new uint[]
    {
        // Front face
        0, 1, 2,
        0, 2, 3,

        // Back face
        4, 5, 6,
        4, 6, 7,

        // Top face
        8, 9, 10,
        8, 10, 11,

        // Bottom face
        12, 13, 14,
        12, 14, 15,

        // Right face
        16, 17, 18,
        16, 18, 19,

        // Left face
        20, 21, 22,
        20, 22, 23
    };
}
