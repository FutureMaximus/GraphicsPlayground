namespace GraphicsPlayground.Graphics.Materials;

public enum MaterialShadingModel
{
    Unlit, // No lighting so no data is provided to the shader
    DefaultLit,
    Custom // Same as no lighting but a way to describe custom lighting
}
