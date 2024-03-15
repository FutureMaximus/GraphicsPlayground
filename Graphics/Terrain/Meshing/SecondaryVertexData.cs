using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Terrain.Meshing;

public struct SecondaryVertexData
{
    public Vector3 Position;
    public ushort VertexMask;
    public ushort VertexIndex;
}
