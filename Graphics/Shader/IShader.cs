namespace GraphicsPlayground.Graphics.Shaders;

public interface IShader : IDisposable
{
    ShaderHandler ShaderHandler { get; }
    string Name { get; }
    int ProgramHandle { get; }
    void Use();
}
