﻿using System.Runtime.CompilerServices;
using GraphicsPlayground.Graphics.Terrain.World;
using GraphicsPlayground.Util;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Terrain.Density;

/// <summary>Generates density data for a chunk using noise.</summary>
public class DensityGenerator(GeneratorSettings settings)
{
    /// <summary>The noise used to generate the heightmap.</summary>
    public Noise HeightMapNoise = settings.HeightmapNoise;
    /// <summary>The noise used to generate the density.</summary>
    public Noise Noise3D = settings.DensityNoise;
    /// <summary>The noise used to generate caves.</summary>
    public Noise Cave3D = settings.CaveNoise;
    /// <summary>The strength of the heightmap noise.</summary>
    public float HeightScale = settings.HeightmapStrength;
    /// <summary>The strength of the density noise.</summary>
    public float NoiseScale = settings.DensityStrength;
    /// <summary>The strength of the cave noise.</summary>
    public float CaveScale = settings.CaveStrength;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetPlanetValue(float x, float y, float z, float planetRadius)
    {
        float heightMap = HeightMapNoise.GetNoise(x, z) * HeightScale;
        float noise3D = Noise3D.GetNoise(x, y, z) * NoiseScale;
        float sphere = MathF.Sqrt(x * x + y * y + z * z) - planetRadius;
        return sphere + heightMap + noise3D;
    }
}
