using GraphicsPlayground.Graphics.Terrain.Chunks;
using GraphicsPlayground.Graphics.Terrain.World;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Terrain.Density;

/// <summary> A task that updates the density of the terrain. </summary>
public struct DensityTask
{
    public DensityGenerator Generator;
    public Vector3i ChunkPosition;
    public bool IsComplete;
    public float[] DensityData;
    public int Size;
    public int Step;
    public int StartX;
    public int StartY;
    public int StartZ;

    public DensityTask(WorldSettings worldSettings, GeneratorSettings settings, in ChunkUpdate chunkUpdate)
    {
        int lod = chunkUpdate.LOD;
        int lodScale = 1 << lod;
        int chunkExtents = (worldSettings.ChunkSize >> 1) << lod;
        Vector3i chunkMin = chunkUpdate.Position - new Vector3i(chunkExtents);
        Vector3i startSamplePosition = chunkMin * new Vector3i(1) * lodScale;
        int densitySize = worldSettings.ChunkSize + 3;
        DensityData = new float[densitySize * densitySize * densitySize];
        Size = densitySize;
        Step = lodScale;
        StartX = startSamplePosition.X;
        StartY = startSamplePosition.Y;
        StartZ = startSamplePosition.Z;
        Generator = new DensityGenerator(settings);
        ChunkPosition = chunkUpdate.Position;
        IsComplete = false;
    }

    public readonly void Execute(int index)
    {
        int z = index % Size;
        int y = index / Size % Size;
        int x = index / (Size * Size);
        DensityData[index] = Generator.GetValue(StartX + x * Step, StartY + y * Step, StartZ + z * Step);
    }
}
