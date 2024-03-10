using OpenTK.Mathematics;

namespace GraphicsPlayground.Util;

public struct IntBoundingBox()
{
    public Vector3i Min;
    public Vector3i Max;

    public IntBoundingBox(Vector3i min, Vector3i max) : this()
    {
        Min = min;
        Max = max;
    }

    public IntBoundingBox(Vector3i position, int extends) : this()
    {
        Min = position - new Vector3i(extends);
        Max = position + new Vector3i(extends);
    }

    /// <summary>Returns whether the bounding box contains the given point.</summary>
    public readonly bool Contains(int x, int y, int z)
    {
        return (x >= Min.X && x < Max.X)
            && (y >= Min.Y && y < Max.Y)
            && (z >= Min.Z && z < Max.Z);
    }

    /// <summary>Returns whether the bounding box contains the given point.</summary>
    public readonly bool Contains(Vector3i point)
    {
        return Contains(point.X, point.Y, point.Z);
    }
}
