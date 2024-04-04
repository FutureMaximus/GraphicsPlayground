using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace GraphicsPlayground.Graphics.Shaders.Data;

/// <summary>The screen to view data for the compute shader.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct Screen2View
{
    public Matrix4 InverseProjection;
    public Vector4i TileSizes;
    public Vector2i ScreenSize;
    public float SliceScalingFactor;
    public float SliceBiasFactor;
}
