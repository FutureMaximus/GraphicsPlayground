using GraphicsPlayground.Graphics.Terrain.Meshing;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Terrain.Chunks;

/// <summary>Represents a renderable chunk in the world.</summary>
public sealed class Chunk
{
    public List<TerrainMesh> TerrainMeshes = [];
    public Vector3i Position;
    public Vector3i MinPosition;
    public int LOD;
    public int NeighborsMask;
}
