using System.Runtime.InteropServices;
using OpenTK.Mathematics;


namespace GraphicsPlayground.Graphics.Shaders.Data;

/// <summary>
/// Light data that is sent to the GPU.
/// We need to use a struct for memory alignment.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct GPUPointLightData()
{
    public Vector3 Position;
    public float MaxRange;
    public Vector3 Color;
    public float Intensity;
    public float Constant;
    public float Linear;
    public float Quadratic;
}
