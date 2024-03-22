namespace GraphicsPlayground.Graphics.Shaders.Data;

public struct OutlineData : IShaderData
{
    public readonly string Name => "OutlineData";

    public bool IsEnabled;

    public OutlineData(bool enabled = false)
    {
        IsEnabled = enabled;
    }
    public OutlineData()
    {
        IsEnabled = false;
    }
}
