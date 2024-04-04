using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Shaders.Data;

public struct PBRLightData : IShaderData
{
    public readonly string Name => nameof(PBRLightData);

    /// <summary>The color of the light.</summary>
    public Vector3 Color;
    /// <summary>The intensity of the light this is multiplied with the attenuation factor.</summary>
    public float Intensity;
    /// <summary>The range of the light.</summary>
    public float Range;
    /// <summary>Whether the light is enabled.</summary>
    public bool Enabled;
}
