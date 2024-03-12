using GraphicsPlayground.Graphics.Terrain.Chunks;
using GraphicsPlayground.Graphics.Terrain.Density;
using GraphicsPlayground.Graphics.Terrain.World;
using GraphicsPlayground.Util;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Terrain.Meshing;

/// <summary>Responsible for updating the meshes of the terrain.</summary>
public class MeshUpdateExecutor
{
    public WorldSettings WorldSettings;
    public WorldState WorldState;
    public GeneratorSettings GeneratorSettings;
    public List<MeshTask> MeshTasks = [];
    public DensityGenerator DensityGenerator;

    public MeshUpdateExecutor(WorldSettings worldSettings, ref WorldState worldState, GeneratorSettings generatorSettings)
    {
        WorldSettings = worldSettings;
        WorldState = worldState;
        GeneratorSettings = generatorSettings;
        DensityGenerator = new(GeneratorSettings);
    }

    public void Start()
    {
        for (int i = 0; i < WorldState.ChunkData.ChunkUpdates.Count; i++)
        {
            StartChunkTask(WorldState.ChunkData.ChunkUpdates[i]);
        }
    }

    public void StartChunkTask(in ChunkUpdate chunkUpdate)
    {
        if (chunkUpdate.UpdateType == ChunkUpdateType.Remove && WorldState.ChunkData.ChunkMap.ContainsKey(chunkUpdate.Position))
        {
            WorldState.ChunkData.ChunksToRemove.Add(chunkUpdate.Position);
        }
        else if (chunkUpdate.UpdateType == ChunkUpdateType.Create)
        {
            MeshTask meshTask = new()
            {
                ChunkPosition = chunkUpdate.Position,
                ChunkPositionMin = CoordinateUtilities.GetChunkMin(chunkUpdate.Position, WorldSettings.ChunkSize, chunkUpdate.LOD),
                LOD = chunkUpdate.LOD,
                NeighborsMask = chunkUpdate.NeighborsMask,
                IsComplete = false
            };
            MeshTasks.Add(meshTask);
        }
    }

    public void Execute()
    {
        for (int i = MeshTasks.Count - 1; i >= 0; i--)
        {
            MeshTask task = MeshTasks[i];
            if (task.IsComplete)
            {
                MeshTasks.RemoveAt(i);
                Vector3i chunkPos = task.ChunkPosition;
                if (WorldState.ChunkData.ChunkMap.ContainsKey(chunkPos))
                {
                    
                }
                continue;
            }
            TransvoxelMesher mesher = new(WorldState.VolumeDensityData[task.ChunkPosition], DensityGenerator)
            {
                ChunkMin = CoordinateUtilities.GetChunkMin(task.ChunkPosition, WorldSettings.ChunkSize, task.LOD),
                ChunkSize = WorldSettings.ChunkSize,
                LOD = task.LOD,
                NeighborsMask = task.NeighborsMask,
                MeshDataContainer = new()
            };
            task.Mesher = mesher;
            task.Execute();
            task.IsComplete = true;
        }
    }
}
