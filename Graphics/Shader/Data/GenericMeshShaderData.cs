namespace GraphicsPlayground.Graphics.Shaders.Data;

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
