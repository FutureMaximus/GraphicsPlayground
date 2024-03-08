using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Terrain;

/// <summary> The terrain AABB used for frustum culling and debugging. </summary>
public class TerrainBoundingBox(Vector3 min, Vector3 max)
{
    public readonly Vector3 Min = min;
    public readonly Vector3 Max = max;

    public bool Intersects(TerrainBoundingBox other)
    {
        return Min.X <= other.Max.X && Max.X >= other.Min.X &&
               Min.Y <= other.Max.Y && Max.Y >= other.Min.Y &&
               Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;
    }
}
