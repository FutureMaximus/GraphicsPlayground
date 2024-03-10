using OpenTK.Mathematics;

namespace GraphicsPlayground.Util;

public struct OctreeNode
{
    public Vector3 Position;
    public int Extents;
    public int Depth;

    public ulong LocationCode;
}