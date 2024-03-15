using GraphicsPlayground.Graphics.Render;
using GraphicsPlayground.Graphics.Terrain.Chunks;
using GraphicsPlayground.Graphics.Terrain.Density;
using GraphicsPlayground.Graphics.Terrain.Meshing;

namespace GraphicsPlayground.Graphics.Terrain.World;

/// <summary>The voxel world that contains all the data for the terrain.</summary>
public class VoxelWorld
{
    public Engine Engine;
    public WorldSettings WorldSettings;
    public GeneratorSettings GeneratorSettings = new();
    public WorldState WorldState;
    /// <summary>Updates the chunks in the world based on the target position.</summary>
    public ChunkUpdateExecutor? ChunkUpdateExecutor;
    /// <summary>
    /// Updates the density of the terrain through noise.
    /// To manually modify the density of the terrain use the VolumeDensityData in WorldState.
    /// </summary>
    public DensityUpdateExecutor? DensityUpdateExecutor;
    /// <summary>Updates the mesh of the chunks in the world.</summary>
    public MeshUpdateExecutor? MeshUpdateExecutor;

    public VoxelWorld(Engine engine, WorldSettings worldSettings, GeneratorSettings generatorSettings)
    {
        Engine = engine;
        WorldState = new WorldState(worldSettings);
        WorldSettings = worldSettings;
        GeneratorSettings = generatorSettings;
    }

    public void Start()
    {
        ChunkUpdateExecutor = new ChunkUpdateExecutor(WorldSettings, ref WorldState);
        ChunkUpdateExecutor.Start();
        ChunkUpdateExecutor.Execute();
        DensityUpdateExecutor = new DensityUpdateExecutor(WorldSettings, ref WorldState, GeneratorSettings);
        DensityUpdateExecutor.Start();
        MeshUpdateExecutor = new MeshUpdateExecutor(Engine, WorldSettings, ref WorldState, GeneratorSettings);
        MeshUpdateExecutor.Start();
        //ChunkUpdateExecutor.ChunkUpdates.Clear();
        //ChunkUpdateExecutor.ChunkUpdatesMap.Clear();
    }

    public void Update()
    {
        //ChunkUpdateExecutor?.Execute();
        DensityUpdateExecutor?.Execute();
        MeshUpdateExecutor?.Execute();
    }
}
