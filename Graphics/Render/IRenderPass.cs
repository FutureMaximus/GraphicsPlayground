namespace GraphicsPlayground.Graphics.Render;

public interface IRenderPass : IDisposable
{
    void Load();

    void Render();

    bool IsEnabled { get; set; }
}
