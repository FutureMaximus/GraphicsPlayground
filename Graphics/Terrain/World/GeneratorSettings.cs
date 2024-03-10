
using System;using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphicsPlayground.Util;

namespace GraphicsPlayground.Graphics.Terrain.World;

public class GeneratorSettings
{
    public Noise HeightmapNoise = new();
    public Noise DensityNoise = new();
    public float HeightmapStrength = 1;
    public float DensityStrength = 1;
}
