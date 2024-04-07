namespace GraphicsPlayground.Graphics.Shaders.Data;

///<summary>Shader data for a generic mesh this is not material data.</summary>
public struct GenericMeshShaderData : IShaderData
{
    public readonly string Name => nameof(GenericMeshShaderData);
    public OutlineData OutlineData;

    public GenericMeshShaderData(OutlineData? outlineData = null)
    {
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
