using GraphicsPlayground.Graphics.Terrain.Chunks;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Terrain.World;

public class WorldState(WorldSettings worldSettings)
{
    public ChunkData ChunkData = new(worldSettings);
    /// <summary>The volume data per chunk this is where you modify the density of the terrain.</summary>
    public Dictionary<Vector3i, List<float>> VolumeDensityData = []; // TODO: Modify this to use the density data class and not using the List<float> directly.
}
