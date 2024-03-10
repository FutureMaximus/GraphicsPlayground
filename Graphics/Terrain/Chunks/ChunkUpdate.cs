using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Terrain.Chunks;

public struct ChunkUpdate
{
    public ChunkUpdateType UpdateType;
    public Vector3i Position;
    public int LOD;
    public int NeighborsMask;
}
