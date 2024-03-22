using GraphicsPlayground.Util;
using OpenTK.Graphics.OpenGL4;

namespace GraphicsPlayground.Graphics.Shaders;

/// <summary>Compute shader used for making parallel computations on the GPU.</summary>
public class ComputeShader : IShader
{
    public ShaderHandler ShaderHandler { get; }
    public string Name { get; }
    public int ProgramHandle { get; }

    public ComputeShader(ShaderHandler handler, string name, string sourceName)
    {
        if (handler.ShaderPath is null)
        {
            throw new NullReferenceException("Compute Shader path is not set.");
        }
        ShaderHandler = handler;
        ShaderHandler.Shaders.Add(name, this);
        Name = name;
        int shaderHandle = GL.CreateShader(ShaderType.ComputeShader);
        string source = File.ReadAllText(Shader.GetShaderFile(sourceName, "comp", handler));
        GL.ShaderSource(shaderHandle, source);
        Shader.CompileShader(shaderHandle, name);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Shader, shaderHandle, name);
        ProgramHandle = GL.CreateProgram();
        GL.AttachShader(ProgramHandle, shaderHandle);
        GL.LinkProgram(ProgramHandle);
        GL.DetachShader(ProgramHandle, shaderHandle);
        GL.DeleteShader(shaderHandle);
        GraphicsUtil.CheckError($"{name} Compute Shader");
    }

    public void Use()
    {
        GL.UseProgram(ProgramHandle);
        GraphicsUtil.CheckError($"{Name} Compute Shader Use");
    }

    public void Dispose()
    {
        GL.DeleteProgram(ProgramHandle);
        GC.SuppressFinalize(this);
    }
}
