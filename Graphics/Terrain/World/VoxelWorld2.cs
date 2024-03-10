using System.Collections.Concurrent;
using GraphicsPlayground.Graphics.Render;

namespace GraphicsPlayground.Graphics.Terrain.World;

//TODO: Use this as replacement once completed.
/// <summary>The voxel world that contains all the data for the terrain.</summary>
public class VoxelWorld2
{
    public Engine Engine;
    public WorldSettings WorldSettings = new();
    public GeneratorSettings GeneratorSettings = new();
    public WorldState WorldState;

    public VoxelWorld2(Engine engine)
    {
        Engine = engine;
        WorldState = new WorldState(WorldSettings);
    }

    public void Start()
    {
        // TODO: Chunk updating tasks
    }
}
