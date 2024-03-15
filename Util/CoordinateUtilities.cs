using OpenTK.Mathematics;

namespace GraphicsPlayground.Util;

/// <summary>Utility methods for working with coordinates.</summary>
public static class CoordinateUtilities
{
    /// <summary>Get the minimum position of a chunk given its position and size.</summary>
    public static Vector3i GetChunkMin(Vector3i chunkPosition, int chunkSize, int lod) => chunkPosition - new Vector3i((chunkSize >> 1) << lod);

    /// <summary>Rounds a number to the nearest power of 2.</summary>
    public static int NextPowerOf2(int n)
    {
        if (n <= 0)
        {
            return 1;
        }
        int msbPos = (int)Math.Ceiling(Math.Log(n, 2));
        return (int)Math.Pow(2, msbPos);
    }
}
