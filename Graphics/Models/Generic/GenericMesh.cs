using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using GraphicsPlayground.Util;
using System.Runtime.InteropServices;
using GraphicsPlayground.Graphics.Shaders.Data;
using System.Runtime.CompilerServices;

namespace GraphicsPlayground.Graphics.Models.Generic;

/// <summary>
/// Mesh data for a generic model.
/// </summary>
public class GenericMesh(string name, GenericModelPart modelPart) : IDisposable
{
    /// <summary> Model part that owns this mesh. </summary>
    public GenericModelPart ParentPart = modelPart;
    public string Name = name;
    public Guid ID => _id;
    private readonly Guid _id = Guid.NewGuid();
    public GenericMeshShaderData ShaderData;

    // ====== Mesh Data ======
    public List<Vector3> Vertices = [];
    public int VerticesLength = 0;
    public List<uint> Indices = [];
    public int IndicesLength = 0;
    public List<Vector2> TextureCoords = [];
    public int TextureCoordsLength = 0;
    public List<Vector3> Normals = [];
    public int NormalsLength = 0;
    public List<int> BoneIDs = new(GraphicsUtil.MaxBoneInfluence);
    public List<float> Weights = new(GraphicsUtil.MaxBoneInfluence);
    public List<Vector3>? Tangents;
    public int TangentsLength = 0;
    public bool HasTangents = false;
    // =======================

    // ====== OpenGL Data =====
    public int VertexBufferObject;
    public int TangentBufferObject; // To prevent too much data being sent to a buffer, we store the tangents in a separate buffer.
    public int BoneBufferObject; // To prevent too much data being sent to a buffer, we store the bone data in a separate buffer.
    public int ElementBufferObject;
    public int VertexArrayObject;
    // ========================

    // ====== Transform Data ======
    public Vector3 Position = Vector3.Zero;
    public Quaternion Rotation = Quaternion.Identity;
    public Vector3 RotationEuler
    {
        get => _rotationEulerStorage;
        set
        {
            _rotationEulerStorage = value;
            Vector3 rad = new(MathHelper.DegreesToRadians(value.X), MathHelper.DegreesToRadians(value.Y), MathHelper.DegreesToRadians(value.Z));
            Rotation = Quaternion.FromEulerAngles(rad).Normalized();
        }
    }
    private Vector3 _rotationEulerStorage = Vector3.Zero;
    public Vector3 Scale = Vector3.One;
    // ============================

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

        /*if (BoneIDs.Count == 0 || Weights.Count == 0)
        {
            // Fill up the bone weights and IDs with empty data.
            for (int i = 0; i < Vertices.Count; i++)
            {
                BoneIDs.AddRange(GraphicsUtil.EmptyBoneIDs());
                Weights.AddRange(GraphicsUtil.EmptyBoneWeights());
            }
        }*/

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
        GL.BufferData(BufferTarget.ArrayBuffer, data.Count * Unsafe.SizeOf<GenericVertexData>(), data.ToArray(), ParentPart.ModelUsageHint);
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
                ParentPart.ModelUsageHint);
            GraphicsUtil.CheckError($"{Name} TangentBufferObject Load");

            // Layout 5: Tangent
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
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, ParentPart.ModelUsageHint);
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

    public void Render()
    {
        GL.BindVertexArray(VertexArrayObject);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
        GL.DrawElements(PrimitiveType.Triangles, IndicesLength, DrawElementsType.UnsignedInt, 0);
        //GraphicsUtil.CheckError($"{Name} Render Call");
        GL.BindVertexArray(0);
    }

    public void Dispose()
    {
        GL.DeleteVertexArray(VertexArrayObject);
        GL.DeleteBuffer(VertexBufferObject);
        GL.DeleteBuffer(TangentBufferObject);
        GL.DeleteBuffer(ElementBufferObject);
        GC.SuppressFinalize(this);
    }

    public override bool Equals(object? obj)
    {
        if (obj is GenericMesh mesh)
        {
            return mesh.ID == ID;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ID);
    }

    public static bool operator ==(GenericMesh left, GenericMesh right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(GenericMesh left, GenericMesh right)
    {
        return !(left == right);
    }
}
