using OpenTK.Graphics.OpenGL4;

namespace GraphicsPlayground.Graphics.Shaders;

public struct ShaderUniform
{
    public string Name;
    public int Location;
    public ActiveUniformType Type;
    public int Size;
}
