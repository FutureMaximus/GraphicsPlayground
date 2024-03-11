using GraphicsPlayground.Graphics.Terrain.Chunks;
using GraphicsPlayground.Graphics.Terrain.World;

namespace GraphicsPlayground.Graphics.Terrain.Density;

/// <summary> Updates the density of the terrain. </summary>
public class DensityUpdateExecutor(WorldSettings worldSettings, WorldState worldState, GeneratorSettings generatorSettings)
{
    public WorldSettings WorldSettings = worldSettings;
    public WorldState WorldState = worldState;
    public GeneratorSettings GeneratorSettings = generatorSettings;
    public List<DensityTask> DensityTasks = [];

    public void Execute()
    {
        for (int i = DensityTasks.Count - 1; i >= 0; i--)
        {
            DensityTask task = DensityTasks[i];
            if (task.IsComplete)
            {
                DensityTasks.RemoveAt(i);

            }
        }
    }

    public void StartTask(in ChunkUpdate chunkUpdate)
    {
        if (chunkUpdate.UpdateType != ChunkUpdateType.Create)
        {
            return;
        }
        DensityTask densityTask = new();
    }
}
