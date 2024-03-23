using GraphicsPlayground.Graphics.Shaders.Data;
using GraphicsPlayground.Util;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Models.Mesh;

public class SkeletalMesh(string name, ModelPart modelPart) : IMesh, IDisposable
{
    public ModelPart ParentPart { get; set; } = modelPart;
    public string Name { get; set; } = name;
    public LODInfo LOD { get; set; } = new LODInfo(0, 0);
    public Guid ID { get; set; } = Guid.NewGuid();
    public BufferUsageHint MeshUsageHint => BufferUsageHint.DynamicDraw;

    public IShaderData ShaderData { get; set; } = new GenericMeshShaderData(); // TODO: SkeletalMeshShaderData?

    // ====== Mesh Data ======
    public List<Vector3> Vertices { get; set; } = [];
    public List<uint> Indices { get; set; } = [];
    public List<Vector2> TextureCoords = [];
    public List<Vector3> Normals = [];
    public List<int> BoneIDs = [];
    public List<float> Weights = [];
    public List<Vector3>? Tangents;
    public int VerticesLength = 0;
    public int IndicesLength = 0;
    public int TextureCoordsLength = 0;
    public int NormalsLength = 0;
    public int TangentsLength = 0;
    public bool HasTangents = false;
    // =======================

    // ====== OpenGL Data =====
    public int VertexBufferObject;
    public int TangentBufferObject; // To prevent too much data being sent to a buffer, we store the tangents in a separate buffer.
    public int ElementBufferObject;
    public int VertexArrayObject;
    // ========================

    public bool IsLoaded = false;

    public void Load()
    {
        if (Vertices.Count == 0 || Indices.Count == 0)
        {
            throw new ArgumentException("Vertices and indices are empty.");
        }
        if (Normals.Count == 0)
        {
            throw new ArgumentException("Normals are empty.");
        }
        if (TextureCoords.Count == 0)
        {
            throw new ArgumentException("Texture coordinates are empty.");
        }

        if (BoneIDs.Count == 0 || Weights.Count == 0)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                BoneIDs.Add(-1);
                Weights.Add(0.0f);
            }
        }

        // Normals validation check we cannot have a zero normal
        for (int i = 0; i < Normals.Count; i++)
        {
            if (Normals[i] == Vector3.Zero)
            {
                Normals[i] = Vector3.UnitY;
            }
        }

        VertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(VertexArrayObject);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.VertexArray, VertexArrayObject, $"{Name} VAO");
    }

    public void Render()
    {

    }

    public void Dispose()
    {
        // TODO: Dispose GPU resources.
        GC.SuppressFinalize(this);
    }
}
