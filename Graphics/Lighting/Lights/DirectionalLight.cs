using OpenTK.Mathematics;
using GraphicsPlayground.Graphics.Shaders.Data;
using GraphicsPlayground.Util;

namespace GraphicsPlayground.Graphics.Lighting.Lights;

/// <summary> The direction of this light is the position normalized. </summary>
public class DirectionalLight : Light
{
    public PBRLightData LightData;

    public DirectionalLight(Vector3 direction, PBRLightData lightData)
    {
        Position = direction.Normalized();
        LightData = lightData;
    }

    public override IShaderData GetShaderData() => LightData;
}
