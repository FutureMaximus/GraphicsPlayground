using System.Runtime.CompilerServices;
using GraphicsPlayground.Graphics.Terrain.World;
using GraphicsPlayground.Util;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Terrain.Density;

/// <summary>Generates density data for a chunk using noise.</summary>
public class DensityGenerator(GeneratorSettings settings)
{
    public Noise HeightMapNoise = settings.HeightmapNoise;
    public Noise Noise3D = settings.DensityNoise;
    public float HeightScale = settings.HeightmapStrength;
    public float NoiseScale = settings.DensityStrength;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetValue(Vector3 position) => GetValue(position.X, position.Y, position.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetValue(float x, float y, float z)
    {
        float plane = -y;
        float heightMap = HeightMapNoise.GetNoise(x, z) * HeightScale;
        float noise3D = Noise3D.GetNoise(x, y, z) * NoiseScale;
        return plane + heightMap + noise3D;
    }
}
