using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace GraphicsPlayground.Graphics.Shaders.Data;

/// <summary>The screen to view data for the compute shader.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct Screen2View
{
    public Matrix4 InverseProjection;
    public uint TileSizeX;
    public uint TileSizeY;
    public uint TileSizeZ;
    public Vector2 TileSizePixels;
    public Vector2 ViewPixelSize;
    public float SliceScalingFactor;
    public float SliceBiasFactor;
}
