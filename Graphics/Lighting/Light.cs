using GraphicsPlayground.Graphics.Shaders.Data;
using OpenTK.Mathematics;
using System.Drawing;

namespace GraphicsPlayground.Graphics.Lighting;

public abstract class Light
{
    /// <summary> The position of the light this is also the direction if directional lighting is used. </summary>
    public Vector3 Position;
    /// <summary> The color of the light. </summary>
    public Color Color;
    /// <summary> The intensity of the light. </summary>
    public float Intensity;
    /// <summary> The shader data for the light. </summary>
    public abstract IShaderData GetShaderData();
    /// <summary> If true, the light will be updated in the graphics pipeline. </summary>
    public bool NeedsUpdate = false;
}
