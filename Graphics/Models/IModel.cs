using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Models;

public interface IModel : IDisposable
{
    string Name { get; set; }

    BufferUsageHint ModelUsageHint { get; }

    Matrix4 Transformation();

    void Load();

    void Render();

    void Unload();
}
