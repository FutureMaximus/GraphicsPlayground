using OpenTK.Mathematics;

namespace GraphicsPlayground.Util;

/// <summary>A node in an octree.</summary>
public struct OctreeNode
{
    public Vector3i Position;
    public int Extents;
    public int Depth;
    public ulong LocationCode;
}