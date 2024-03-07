namespace GraphicsPlayground.Graphics.Models;
    
public interface IModel : IDisposable
{
    string Name { get; set; }

    void Load();

    void Render();

    void Unload();
}
