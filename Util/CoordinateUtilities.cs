using OpenTK.Mathematics;

namespace GraphicsPlayground.Util;

/// <summary>Utility methods for working with coordinates.</summary>
public static class CoordinateUtilities
{
    /// <summary>Get the minimum position of a chunk given its position and size.</summary>
    public static Vector3i GetChunkMin(Vector3i chunkPosition, int chunkSize, int lod) => chunkPosition - new Vector3i((chunkSize >> 1) << lod);
}
