using GraphicsPlayground.Graphics.Textures;

namespace GraphicsPlayground.Graphics.Shaders.Data;

public struct PBRMaterialData : IShaderData
{
    public readonly string Name => nameof(PBRMaterialData);

    /// <summary>
    /// The base texture of the material.
    /// </summary>
    public Texture2D AlbedoTexture;
    /// <summary>
    /// The normal texture of the material.
    /// </summary>
    public Texture2D NormalTexture;
    /// <summary>
    /// The ARM texture of the material.
    /// <para>
    /// A = Ambient Occlusion where the R channel is used.
    /// </para>
    /// <para>
    /// R = Roughness where the G channel is used.
    /// </para>
    /// <para>
    /// M = Metallic where the B channel is used.
    /// </para>
    /// </summary>
    public Texture2D ARMTexture;
    /// <summary> The optional height texture of the material. </summary>
    public Texture2D? HeightTexture;
    /// <summary> The height scale if the height texture is used. </summary>
    public float HeightScale;

    public PBRMaterialData(Texture2D albedoTexture, Texture2D normalTexture, Texture2D armTexture, Texture2D heightTexture, float heightScale = 0.1f)
    {
        AlbedoTexture = albedoTexture;
        NormalTexture = normalTexture;
        ARMTexture = armTexture;
        HeightTexture = heightTexture;
        HeightScale = heightScale;
    }
    public PBRMaterialData(Texture2D albedoTexture, Texture2D normalTexture, Texture2D armTexture)
    {
        AlbedoTexture = albedoTexture;
        NormalTexture = normalTexture;
        ARMTexture = armTexture;
    }
}
