using System.Collections.Concurrent;
using GraphicsPlayground.Graphics.Render;
using GraphicsPlayground.Graphics.Terrain.Chunks;

namespace GraphicsPlayground.Graphics.Terrain.World;

//TODO: Use this as replacement once completed.
/// <summary>The voxel world that contains all the data for the terrain.</summary>
public class VoxelWorld2
{
    public Engine Engine;
    public WorldSettings WorldSettings;
    public GeneratorSettings GeneratorSettings = new();
    public WorldState WorldState;
    /// <summary>Updates the chunks in the world based on the target position.</summary>
    public ChunkUpdateExecutor? ChunkUpdateExecutor;

    public VoxelWorld2(Engine engine, WorldSettings worldSettings)
    {
        Engine = engine;
        WorldState = new WorldState(worldSettings);
        WorldSettings = worldSettings;
    }

    public void Start()
    {
        ChunkUpdateExecutor = new ChunkUpdateExecutor(WorldSettings, WorldState);
        ChunkUpdateExecutor.Start();
        ChunkUpdateExecutor.ChunkUpdates.Clear();
        ChunkUpdateExecutor.ChunkUpdatesMap.Clear();
    }

    public void Update()
    {
        ChunkUpdateExecutor?.Execute();
    }
}
