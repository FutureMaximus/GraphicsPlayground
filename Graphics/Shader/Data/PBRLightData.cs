using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Shaders.Data;

public struct PBRLightData : IShaderData
{
    public readonly string Name => nameof(PBRLightData);

    /// <summary>The color of the light.</summary>
    public Vector3 Color;
    /// <summary>The intensity of the light this is multiplied with the attenuation factor.</summary>
    public float Intensity;
    /// <summary>The maximum range before this light is culled.</summary>
    public float MaxRange;
    /// <summary>Constant attenuation factor.</summary>
    public float Constant;
    /// <summary>Linear attenuation factor.</summary>
    public float Linear;
    /// <summary>Quadratic attenuation factor.</summary>
    public float Quadratic;
    /// <summary>Whether the light is enabled.</summary>
    public bool Enabled;
}
