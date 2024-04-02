using System.Numerics;
using System.Runtime.InteropServices;

namespace GraphicsPlayground.Graphics.Shaders.Data;

/// <summary> A volume tile axis-aligned bounding box. </summary>
[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct VolumeTileAABB
{
    public Vector4 Min;
    public Vector4 Max;
}
