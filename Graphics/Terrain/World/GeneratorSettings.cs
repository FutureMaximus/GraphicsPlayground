using GraphicsPlayground.Util;

namespace GraphicsPlayground.Graphics.Terrain.World;

public class GeneratorSettings
{
    public Noise HeightmapNoise = new();
    public Noise DensityNoise = new();
    public Noise CaveNoise = new();
    public float HeightmapStrength = 5;
    public float DensityStrength = 15;
    public float CaveStrength = 100;
}
