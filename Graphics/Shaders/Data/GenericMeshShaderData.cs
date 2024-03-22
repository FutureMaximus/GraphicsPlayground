namespace GraphicsPlayground.Graphics.Shaders.Data;

///<summary>Shader data for a generic mesh.</summary>
public struct GenericMeshShaderData : IShaderData
{
    public readonly string Name => nameof(GenericMeshShaderData);
    public PBRMaterialData MaterialData;
    public OutlineData OutlineData;

    public GenericMeshShaderData(PBRMaterialData materialData, OutlineData? outlineData = null)
    {
        MaterialData = materialData;
        if (outlineData.HasValue)
        {
            OutlineData = outlineData.Value;
        }
        else
        {
            OutlineData = new OutlineData();
        }
    }
}
