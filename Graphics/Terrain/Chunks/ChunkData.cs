using OpenTK.Mathematics;
using GraphicsPlayground.Util;
using GraphicsPlayground.Graphics.Terrain.World;

namespace GraphicsPlayground.Graphics.Terrain.Chunks;

public class ChunkData
{
    public List<ChunkUpdate> ChunkUpdates;
    public List<ChunkUpdate> FilteredChunkUpdates;
    public List<bool> ChunkUniformState;

    public Dictionary<Vector3, Chunk> ChunkMap;
    public LinearOctree ChunkTree;
    public List<Vector3> ChunksToRemove;
    public List<Chunk> PendingChunks;
    public List<TerrainMesh> MeshesToClear;

    public ChunkData(WorldSettings worldSettings)
    {
        int rootDepth = (int)Math.Round(Math.Log(worldSettings.WorldSize / worldSettings.ChunkSize) / 2);
        ChunkMap = [];
        ChunkTree = new LinearOctree(Vector3i.Zero, worldSettings.WorldSize, rootDepth);
        PendingChunks = [];
        ChunksToRemove = [];
        MeshesToClear = [];
        ChunkUpdates = [];
        FilteredChunkUpdates = [];
        ChunkUniformState = [];
    }
}
