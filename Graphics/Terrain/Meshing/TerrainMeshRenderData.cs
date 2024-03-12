using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Terrain.Meshing;

public class TerrainMeshRenderData
{
    public int VertexArrayObject;
    public int VertexBufferObject;
    public int ElementBufferObject;

    public List<Vector3> Vertices = [];
    public List<Vector3> Normals = [];
    public List<uint> Indices = [];
    public List<SecondaryVertexData> SecondaryVertices = [];

    public void Clear()
    {
        Vertices.Clear();
        Normals.Clear();
        Indices.Clear();
    }

    public void Dispose()
    {
        GL.DeleteVertexArray(VertexArrayObject);
        GL.DeleteBuffer(VertexBufferObject);
        GL.DeleteBuffer(ElementBufferObject);
    }
}
