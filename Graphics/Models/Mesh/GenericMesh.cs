using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using GraphicsPlayground.Util;
using System.Runtime.InteropServices;
using GraphicsPlayground.Graphics.Shaders.Data;
using System.Runtime.CompilerServices;
using GraphicsPlayground.Graphics.Models.Generic;
using GraphicsPlayground.Graphics.Materials;
using System.Diagnostics.CodeAnalysis;
using GraphicsPlayground.Graphics.Render;
using System.Text;
using GraphicsPlayground.Graphics.Shaders;

namespace GraphicsPlayground.Graphics.Models.Mesh;

public class GenericMesh(string name, ModelPart modelPart) : IMesh
{
    /// <summary> Model part that owns this mesh. </summary>
    public ModelPart ParentPart { get; set; } = modelPart;
    public string Name { get; set; } = name;
    public LODInfo LOD { get; set; } = new LODInfo(0, 0);
    public Guid ID { get; set; } = Guid.NewGuid();
    public IShaderData ShaderData { get; set; } = new GenericMeshShaderData();
    public BufferUsageHint MeshUsageHint { get; set; } = BufferUsageHint.StaticDraw;
    public Material? Material { get; set; }

    // ====== Mesh Data ======
    public List<Vector3> Vertices { get; set; } = [];
    public int VerticesLength = 0;
    public List<uint> Indices { get; set; } = [];
    public int IndicesLength = 0;
    public List<Vector2> TextureCoords = [];
    public int TextureCoordsLength = 0;
    public List<Vector3> Normals = [];
    public int NormalsLength = 0;
    public List<Vector3>? Tangents;
    public int TangentsLength = 0;
    public bool HasTangents { get; set; } = false;
    // =======================

    // ====== OpenGL Data =====
    public int VertexBufferObject;
    public int TangentBufferObject; // To prevent too much data being sent to a buffer, we store the tangents in a separate buffer.
    public int BoneBufferObject; // To prevent too much data being sent to a buffer, we store the bone data in a separate buffer.
    public int ElementBufferObject;
    public int VertexArrayObject;
    // ========================

    public bool IsLoaded { get; set; } = false;

    public Guid MeshID => throw new NotImplementedException();

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

        // ========= Vertex Binding ========
        List<GenericVertexData> data = GeometryHelper.GetVertexDatas(Vertices, Normals, TextureCoords);
        if (HasTangents != false)
        {
            Tangents ??= GeometryHelper.CalculateTangents(Vertices, TextureCoords, Normals, Indices);
            if (Tangents is not null)
            {
                HasTangents = true;
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

        VertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, VertexBufferObject, $"{Name} VBO");
        GL.BufferData(BufferTarget.ArrayBuffer, data.Count * Unsafe.SizeOf<GenericVertexData>(), data.ToArray(), MeshUsageHint);
        GraphicsUtil.CheckError($"{Name} VBO Load");

        int stride = Marshal.SizeOf(typeof(GenericVertexData));

        // Layout 0: Position
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(0);

        // Layout 1: Normal
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        // Layout 2: Texture Coordinates
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));
        GL.EnableVertexAttribArray(2);

        /*// Layout 3: Bone IDs
        GL.VertexAttribIPointer(3, 4, VertexAttribIntegerType.Int, stride, 8 * sizeof(float));
        GL.EnableVertexAttribArray(3);

        // Layout 4: Weights
        GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, stride, 12 * sizeof(float));
        GL.EnableVertexAttribArray(4);*/

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        if (HasTangents)
        {
            TangentBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, TangentBufferObject);
            GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, TangentBufferObject, $"{Name} TangentBufferObject");
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                Tangents?.Count * Marshal.SizeOf(typeof(Vector3)) ?? 0,
                Tangents?.ToArray() ?? [],
                MeshUsageHint);
            GraphicsUtil.CheckError($"{Name} TangentBufferObject Load");

            // Layout 3: Tangent
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf(typeof(Vector3)), 0);
            GL.EnableVertexAttribArray(3);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        GraphicsUtil.CheckError($"{Name} VAO Load");

        //=================================

        // ========== Element Buffer Binding ==========
        ElementBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
        uint[] indices = [.. Indices];
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, MeshUsageHint);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, ElementBufferObject, $"{Name} EBO");
        GraphicsUtil.CheckError($"{Name} EBO Load");
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        // ============================================

        GL.BindVertexArray(0);
        VerticesLength = GeometryHelper.ArrayFromVector3List(Vertices).Length;
        IndicesLength = indices.Length;
        TextureCoordsLength = GeometryHelper.ArrayFromVector2List(TextureCoords).Length;
        NormalsLength = GeometryHelper.ArrayFromVector3List(Normals).Length;
        if (Tangents is not null)
        {
            TangentsLength = GeometryHelper.ArrayFromVector3List(Tangents).Length;
        }
        IsLoaded = true;
    }

    /// <summary>Updates the mesh.</summary>
    public void Update(in Engine engine)
    {
        // Nothing to update for generic mesh.
    }

    public void Render()
    {
        GL.BindVertexArray(VertexArrayObject);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
        GL.DrawElements(PrimitiveType.Triangles, IndicesLength, DrawElementsType.UnsignedInt, 0);
        //GraphicsUtil.CheckError($"{Name} Render Call");
        GL.BindVertexArray(0);
    }

    /// <summary>Builds the material for the mesh.</summary>
    public void BuildMaterial(in Engine engine)
    {
        if (engine.ShaderHandler == null)
        {
            throw new Exception("Engine must have a shader handler to build materials.");
        }
        if (Material is null)
        {
            throw new Exception("Material is null cannot build.");
        }
        List<string> directives = [];
        StringBuilder sb = new();
        if (Material is PBRMaterial pbrMaterial)
        {
            PBRMaterial.Process(pbrMaterial, directives, sb);
            Material.ShaderProgram = new ShaderProgram(engine.ShaderHandler, Name, "pbr_cluster", sb.ToString());
        }
        else
        {
            throw new Exception("Material type not supported for generic mesh.");
        }
    }

    public void Dispose()
    {
        GL.DeleteVertexArray(VertexArrayObject);
        GL.DeleteBuffer(VertexBufferObject);
        GL.DeleteBuffer(TangentBufferObject);
        GL.DeleteBuffer(ElementBufferObject);
        GC.SuppressFinalize(this);
    }

    public bool Equals(IMesh? x, IMesh? y)
    {
        if (x is null || y is null)
        {
            return false;
        }
        return x.ID == y.ID;
    }

    public int GetHashCode([DisallowNull] IMesh obj)
    {
        return HashCode.Combine(ID);
    }
}
