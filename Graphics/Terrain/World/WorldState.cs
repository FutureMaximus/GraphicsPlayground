using GraphicsPlayground.Graphics.Terrain.Chunks;

namespace GraphicsPlayground.Graphics.Terrain.World;

public class WorldState(WorldSettings worldSettings)
{
    public ChunkData ChunkData = new(worldSettings);
    /// <summary>The volume data for the world this is where you modify the terrain.</summary>
    public readonly IVolumeData<sbyte> VolumeDensityData = new VolumeDictionary<sbyte>(new VolumeSize(8));
}
