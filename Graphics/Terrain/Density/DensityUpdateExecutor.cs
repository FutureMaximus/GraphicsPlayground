using GraphicsPlayground.Graphics.Terrain.Chunks;
using GraphicsPlayground.Graphics.Terrain.World;

namespace GraphicsPlayground.Graphics.Terrain.Density;

/// <summary> Updates the density of the terrain. </summary>
public class DensityUpdateExecutor(WorldSettings worldSettings, ref WorldState worldState, GeneratorSettings generatorSettings)
{
    public WorldSettings WorldSettings = worldSettings;
    public WorldState WorldState = worldState;
    public GeneratorSettings GeneratorSettings = generatorSettings;
    public List<DensityTask> DensityTasks = [];

    public void Start()
    {
        int updateCount = WorldState.ChunkData.ChunkUpdates.Count;
        for (int i = updateCount - 1; i >= 0; i--)
        {
            //Console.WriteLine($"Starting density task {i}");
            StartTask(WorldState.ChunkData.ChunkUpdates[i]); // TODO: Multi-thread this.
        }
    }

    public void Execute()
    {
        for (int i = DensityTasks.Count - 1; i >= 0; i--)
        {
            DensityTask task = DensityTasks[i];
            if (task.IsComplete)
            {
                DensityTasks.RemoveAt(i);
                WorldState.VolumeDensityData[task.ChunkPosition] = [.. task.DensityData];
            }
        }
    }

    public void StartTask(in ChunkUpdate chunkUpdate)
    {
        if (chunkUpdate.UpdateType != ChunkUpdateType.Create)
        {
            return;
        }
        DensityTask densityTask = new(WorldSettings, GeneratorSettings, chunkUpdate);
        densityTask.IsComplete = true; // TODO: Move this below when we have multi-threading the separate thread should set this to true.
        DensityTasks.Add(densityTask);
        int densitySize = WorldSettings.ChunkSize + 3;
        int densitySizeCubed = densitySize * densitySize * densitySize;
        for (int i = 0; i < densitySizeCubed; i++)
        {
            densityTask.Execute(i);
        }
        // Set IsComplete to true here when we have multi-threading.
    }
}
