using GraphicsPlayground.Graphics.Terrain.Chunks;
using GraphicsPlayground.Graphics.Terrain.World;
using GraphicsPlayground.Util;

namespace GraphicsPlayground.Graphics.Terrain.Meshing;

/// <summary>Responsible for updating the meshes of the terrain.</summary>
public class MeshUpdateExecutor
{
    public WorldSettings WorldSettings;
    public WorldState WorldState;
    public GeneratorSettings GeneratorSettings;
    public List<MeshTaskData> MeshTasks = [];

    public MeshUpdateExecutor(WorldSettings worldSettings, ref WorldState worldState, GeneratorSettings generatorSettings)
    {
        WorldSettings = worldSettings;
        WorldState = worldState;
        GeneratorSettings = generatorSettings;
    }

    public void StartChunkTask(in ChunkUpdate chunkUpdate)
    {
        if (chunkUpdate.UpdateType == ChunkUpdateType.Remove && WorldState.ChunkData.ChunkMap.ContainsKey(chunkUpdate.Position))
        {
            WorldState.ChunkData.ChunksToRemove.Add(chunkUpdate.Position);
        }
        else if (chunkUpdate.UpdateType == ChunkUpdateType.Create)
        {
            MeshTaskData meshTaskData = new()
            {
                ChunkPosition = chunkUpdate.Position,
                ChunkPositionMin = CoordinateUtilities.GetChunkMin(chunkUpdate.Position, WorldSettings.ChunkSize, chunkUpdate.LOD),
                LOD = chunkUpdate.LOD,
                NeighborsMask = chunkUpdate.NeighborsMask
            };
            MeshTasks.Add(meshTaskData);
        }
    }
}
