namespace GraphicsPlayground.Graphics.Shaders;

public class ShaderHandler(string shaderPath) : IDisposable
{
    public Dictionary<string, IShader> Shaders = [];
    public string[] ShaderFiles = Directory.GetFiles(shaderPath);
    public string ShaderPath = shaderPath;

    public void AddShader(IShader shader)
    {
        if (ShaderPath is null)
        {
            throw new NullReferenceException("ShaderProgram path is not set.");
        }
        if (Shaders.ContainsKey(shader.Name))
        {
            throw new ArgumentNullException($"ShaderProgram with name {shader.Name} already exists.");
        }
        Shaders.Add(shader.Name, shader);
    }

    public IShader? GetShader(string name)
    {
        if (ShaderPath is null)
        {
            throw new NullReferenceException("ShaderProgram path is not set.");
        }

        if (Shaders.TryGetValue(name, out IShader? shader))
        {
            return shader;
        }

        return null;
    }

    public void Dispose()
    {
        foreach (IShader shader in Shaders.Values)
        {
            shader.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
