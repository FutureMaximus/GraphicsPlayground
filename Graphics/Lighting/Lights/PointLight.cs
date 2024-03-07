using OpenTK.Mathematics;
using GraphicsPlayground.Graphics.Shaders.Data;

namespace GraphicsPlayground.Graphics.Lighting.Lights;

public class PointLight : Light
{
    public PBRLightData LightData;

    public PointLight(Vector3 position, PBRLightData lightData)
    {
        Position = position;
        LightData = lightData;
    }

    public override IShaderData GetShaderData() => LightData;
}
