using GraphicsPlayground.Graphics.Models;
using GraphicsPlayground.Util;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace GraphicsPlayground.Graphics.Terrain.Meshing;

public sealed class TerrainMesh(int x, int y, int z) : IDisposable
{
    public readonly Vector3 Position = new(x, y, z);
    public Vector3 Scale = new(1);
    public readonly Matrix4 Translation = Matrix4.CreateTranslation(x, y, z);
    public int LOD = 1;

    public List<Vector3> Vertices = [];
    public List<Vector3> Normals = [];
    public List<uint> Indices = [];
    public int VerticesLength = 0;
    public int NormalsLength = 0;
    public int IndicesLength = 0;

    public int VertexBufferObject;
    public int ElementBufferObject;
    public int VertexArrayObject;

    public bool IsEmpty => Vertices.Count == 0 || Indices.Count == 0 || Normals.Count == 0;
    public bool IsLoaded = false;

    /// <summary > Loads the mesh into the GPU and returns true if successful. </summary>
    public void Load()
    {
        if (IsEmpty)
        {
            return;
        }

        string terrainMeshID = $"Terrain Grid ({Position.X},{Position.Y},{Position.Z}) Mesh";

        // ======== Vertex Binding =========
        VertexArrayObject = GL.GenVertexArray();

        GL.BindVertexArray(VertexArrayObject);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.VertexArray, VertexArrayObject, $"{terrainMeshID} VAO");

        VertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, VertexBufferObject, $"{terrainMeshID} VBO");
        float[] data = GeometryHelper.InterleavedArrayPositionNormals(
             Vertices, Normals);
        GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.StaticDraw);
        GraphicsUtil.CheckError($"{terrainMeshID} VBO Load");

        int stride = Marshal.SizeOf(typeof(Vector3)) * 2;

        // Layout 0: Position
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(0);

        // Layout 1: Normals
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, Marshal.SizeOf(typeof(Vector3)));
        GL.EnableVertexAttribArray(1);

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        // =================================

        // ======== Index Binding ==========
        ElementBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, ElementBufferObject, $"{terrainMeshID} EBO");
        uint[] indices = [.. Indices];
        IndicesLength = indices.Length;
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
        GraphicsUtil.CheckError($"{terrainMeshID} EBO Load");
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

        GL.BindVertexArray(0);
        // =================================

        VerticesLength = data.Length;
        IsLoaded = true;
        return;
    }

    public void Render()
    {
        if (!IsLoaded)
        {
            Load();
        }
        GL.BindVertexArray(VertexArrayObject);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
        GL.DrawElements(PrimitiveType.Triangles, IndicesLength, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    }

    public ushort LatestAddedVertIndex()
    {
        return (ushort)(Vertices.Count - 1);
    }

    public void Dispose()
    {
        Vertices.Clear();
        Normals.Clear();
        Indices.Clear();
        GL.DeleteVertexArray(VertexArrayObject);
        GL.DeleteBuffer(VertexBufferObject);
        GL.DeleteBuffer(ElementBufferObject);
        GC.SuppressFinalize(this);
    }
}
