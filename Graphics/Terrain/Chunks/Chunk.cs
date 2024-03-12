using GraphicsPlayground.Graphics.Terrain.Meshing;
using System.Numerics;

namespace GraphicsPlayground.Graphics.Terrain.Chunks;

/// <summary>Represents a renderable chunk in the world.</summary>
public sealed class Chunk
{
    public List<TerrainMesh> TerrainMeshes = [];
    public Vector3 Position;
    public int LOD;
    public int NeighborsMask;
}
