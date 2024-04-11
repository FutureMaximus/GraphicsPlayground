﻿using GraphicsPlayground.Graphics.Animations;
using GraphicsPlayground.Graphics.Shaders.Data;
using GraphicsPlayground.Util;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using GraphicsPlayground.Graphics.Shader.Data;
using GraphicsPlayground.Graphics.Materials;
using System.Diagnostics.CodeAnalysis;
using GraphicsPlayground.Graphics.Render;
using GraphicsPlayground.Graphics.Materials.Properties;
using GraphicsPlayground.Graphics.Shaders;
using System.Text;

namespace GraphicsPlayground.Graphics.Models.Mesh;

/// <summary> A skeletal mesh that contains a hierarchy of bones that can be animated. </summary>
public class SkeletalMesh(string name, ModelPart modelPart) : IMesh, IDisposable
{
    /// <summary> The animator that will animate this mesh.</summary>
    public Animator? Animator;
    /// <summary> Model part that owns this mesh. </summary>
    public ModelPart ParentPart { get; set; } = modelPart;
    /// <summary> Name of the mesh. </summary>
    public string Name { get; set; } = name;
    /// <summary>The skeletal bones of the mesh.</summary>
    public Dictionary<string, BoneInfo> BoneInfoMap { get; set; } = [];
    /// <summary> The amount of bones in the mesh. </summary>
    public int BoneCounter = 0;
    /// <summary> Level of detail information. </summary>
    public LODInfo LOD { get; set; } = new LODInfo(0, 0);
    /// <summary> Unique identifier of the mesh. </summary>
    public Guid ID { get; set; } = Guid.NewGuid();
    /// <summary> Usage hint for the mesh. </summary>
    public BufferUsageHint MeshUsageHint => BufferUsageHint.DynamicDraw;
    /// <summary> Shader data for rendering the mesh. </summary>
    public IShaderData ShaderData { get; set; } = new GenericMeshShaderData(); // TODO: SkeletalMeshShaderData?
    /// <summary> ParentMaterial to render for this mesh. </summary>
    public Material? Material { get; set; }

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
    public bool HasTangents { get; set; } = false;
    // =======================

    // ====== OpenGL Data =====
    public int VertexBufferObject;
    public int TangentBufferObject; // To prevent too much data being sent to a buffer, we store the tangents in a separate buffer.
    public int ElementBufferObject;
    public int VertexArrayObject;
    // ========================

    public bool IsLoaded { get; set; } = false;

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

        // ========= Vertex Binding ========
        List<SkeletalVertexData> data = GeometryHelper.GetSkeletalVertexDatas(Vertices, Normals, TextureCoords, BoneIDs, Weights);

        VertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(VertexArrayObject);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.VertexArray, VertexArrayObject, $"{Name} Skeletal Mesh VAO");

        VertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, VertexBufferObject, $"{Name} Skeletal Mesh VBO");
        GL.BufferData(BufferTarget.ArrayBuffer, data.Count * Unsafe.SizeOf<SkeletalVertexData>(), data.ToArray(), MeshUsageHint);
        GraphicsUtil.CheckError($"{Name} VBO Load");

        int stride = Unsafe.SizeOf<SkeletalVertexData>();

        // Layout 0: Position
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(0);

        // Layout 1: Normal
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        // Layout 2: Texture Coordinates
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));
        GL.EnableVertexAttribArray(2);

        // Layout 3: Bone IDs
        GL.VertexAttribIPointer(3, 4, VertexAttribIntegerType.Int, stride, 8 * sizeof(float));
        GL.EnableVertexAttribArray(3);

        // Layout 4: Weights
        GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, stride, 12 * sizeof(float));
        GL.EnableVertexAttribArray(4);

        // TODO: Remove extra buffer
        TangentBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, TangentBufferObject);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, TangentBufferObject, $"{Name} Skeletal Mesh TangentBufferObject");
        GL.BufferData(
            BufferTarget.ArrayBuffer,
            Tangents?.Count * Unsafe.SizeOf<Vector3>() ?? 0,
            Tangents?.ToArray() ?? [],
            MeshUsageHint);
        GraphicsUtil.CheckError($"{Name} Skeletal Mesh TangentBufferObject Load");

        // Layout 5: Tangent
        GL.VertexAttribPointer(5, 3, VertexAttribPointerType.Float, false, Unsafe.SizeOf<Vector3>(), 0);
        GL.EnableVertexAttribArray(3);

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        GraphicsUtil.CheckError($"{Name} Skeletal Mesh VAO Load");

        //=================================

        // ========== Element Buffer Binding ==========
        ElementBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
        uint[] indices = [.. Indices];
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, MeshUsageHint);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, ElementBufferObject, $"{Name} Skeletal Mesh EBO");
        GraphicsUtil.CheckError($"{Name} Skeletal Mesh EBO Load");
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        // ============================================

        GL.BindVertexArray(0);
        VerticesLength = GeometryHelper.ArrayFromVector3List(Vertices).Length;
        IndicesLength = indices.Length;
        TextureCoordsLength = GeometryHelper.ArrayFromVector2List(TextureCoords).Length;
        NormalsLength = GeometryHelper.ArrayFromVector3List(Normals).Length;
        TangentsLength = GeometryHelper.ArrayFromVector3List(Tangents ?? []).Length;
        IsLoaded = true;
    }

    public void Update(in Engine engine)
    {
        Animator?.UpdateAnimation(engine.DeltaTime);
    }

    public void Render()
    {
        GL.BindVertexArray(VertexArrayObject);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
        GL.DrawElements(PrimitiveType.Triangles, IndicesLength, DrawElementsType.UnsignedInt, 0);
        //GraphicsUtil.CheckError($"{Name} Render Call");
        GL.BindVertexArray(0);
    }

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
        directives.Add("#define SKELETAL_MESH");
        if (Material is PBRMaterial pbrMaterial)
        {
            PBRMaterial.Process(pbrMaterial, directives, sb);
            List<Matrix4> transforms = [];
            foreach (BoneInfo boneInfo in BoneInfoMap.Values)
            {
                transforms.Add(boneInfo.Offset);
            }
            Material.Properties.Add(new MaterialMatrixArrayProperty("finalBonesMatrices", transforms));
            Material.ShaderProgram = new ShaderProgram(engine.ShaderHandler, Material.Name, "pbr_cluster", sb.ToString());
        }
        else
        {
            throw new Exception("Material type not supported for skeletal mesh.");
        }
    }

    public void Dispose()
    {
        // TODO: Dispose GPU resources.
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
