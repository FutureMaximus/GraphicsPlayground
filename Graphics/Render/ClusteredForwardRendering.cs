namespace GraphicsPlayground.Graphics.Render;

public class ClusteredForwardRendering : IRenderPass
{
    public bool IsEnabled { get; set; }
    public Engine Engine;

    public ClusteredForwardRendering(Engine engine)
    {
        Engine = engine;
    }

    public void Load()
    {
        throw new NotImplementedException();
    }

    public void Render()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
