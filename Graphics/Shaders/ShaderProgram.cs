using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using GraphicsPlayground.Util;

namespace GraphicsPlayground.Graphics.Shaders;

/// <summary>
/// ShaderProgram class for handling general OpenGL shaders with vertex, 
/// fragment, and optional geometry, tessellation control, and tessellation evaluation shaders.
/// </summary>
public sealed class ShaderProgram : IShader
{
    /// <summary> The shader handler that owns this shader. </summary>
    public ShaderHandler ShaderHandler { get; }
    public string Name { get; }
    public int ProgramHandle { get; }

    /// <summary>
    /// Creates a new shader program with the given name and source name.
    /// </summary>
    /// <param name="handler">The global list of known shaders.</param>
    /// <param name="name">The name of the shader.</param>
    /// <param name="sourceName">The source name of the shader this is the file name to look for.</param>
    /// <param name="directives">Precursor directives to add to the shader.</param>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="FileLoadException"></exception>
    public ShaderProgram(ShaderHandler handler, string name, string sourceName, string directives = "")
    {
        if (handler.ShaderPath is null)
        {
            throw new NullReferenceException("ShaderProgram path is not set.");
        }

        Name = name;
        ShaderHandler = handler;
        ShaderHandler.Shaders.Add(Name, this);

        // Vertex shader
        string vertexSource;
        int vertexShader;
        try
        {
            vertexSource = ProcessShaderSource(File.ReadAllText(GetShaderFile(sourceName, "vert", handler)), directives);
            vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexSource);
            CompileShader(vertexShader, name);
        }
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException($"Vertex source {sourceName} for shader {Name} could not be found.");
        }

        // Tessellation Control shader
        string tessControlSource = string.Empty;
        int tessControlShader = -1;
        try
        {
            tessControlSource = ProcessShaderSource(File.ReadAllText(GetShaderFile(sourceName, "tesc", handler)), directives);
            tessControlShader = GL.CreateShader(ShaderType.TessControlShader);
            GL.ShaderSource(tessControlShader, tessControlSource);
            CompileShader(tessControlShader, name);
        }
        catch (FileNotFoundException) { }

        // Tessellation Evaluation shader
        string tessEvalSource = string.Empty;
        int tessEvalShader = -1;
        try
        {
            tessEvalSource = ProcessShaderSource(File.ReadAllText(GetShaderFile(sourceName, "tese", handler)), directives);
            tessEvalShader = GL.CreateShader(ShaderType.TessEvaluationShader);
            GL.ShaderSource(tessEvalShader, tessEvalSource);
            CompileShader(tessEvalShader, name);
        }
        catch (FileNotFoundException) { }

        // Geometry shader
        string geometrySource = string.Empty;
        int geometryShader = -1;
        try
        {
            geometrySource = ProcessShaderSource(File.ReadAllText(GetShaderFile(sourceName, "geom", handler)), directives);
            geometryShader = GL.CreateShader(ShaderType.GeometryShader);
            GL.ShaderSource(geometryShader, geometrySource);
            CompileShader(geometryShader, name);
        }
        catch (FileNotFoundException) { }

        // Fragment shader
        string fragmentSource;
        int fragmentShader;
        try
        {
            fragmentSource = ProcessShaderSource(File.ReadAllText(GetShaderFile(sourceName, "frag", handler)), directives);
            fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentSource);
            CompileShader(fragmentShader, name);
        }
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException($"Fragment source {sourceName} for shader {Name} could not be found.");
        }

        ProgramHandle = GL.CreateProgram();

        // Attach shaders
        GL.AttachShader(ProgramHandle, vertexShader);
        if (tessControlSource != string.Empty && tessControlShader != -1)
        {
            GL.AttachShader(ProgramHandle, tessControlShader);
        }
        if (tessEvalSource != string.Empty && tessEvalShader != -1)
        {
            GL.AttachShader(ProgramHandle, tessEvalShader);
        }
        if (geometrySource != string.Empty && geometryShader != -1)
        {
            GL.AttachShader(ProgramHandle, geometryShader);
        }
        GL.AttachShader(ProgramHandle, fragmentShader);

        LinkProgram(ProgramHandle, name);

        // Detach shaders
        GL.DetachShader(ProgramHandle, vertexShader);
        if (tessControlSource != string.Empty && tessControlShader != -1)
        {
            GL.DetachShader(ProgramHandle, tessControlShader);
        }
        if (tessEvalSource != string.Empty && tessEvalShader != -1)
        {
            GL.DetachShader(ProgramHandle, tessEvalShader);
        }
        if (geometrySource != string.Empty && geometryShader != -1)
        {
            GL.DetachShader(ProgramHandle, geometryShader);
        }
        GL.DetachShader(ProgramHandle, fragmentShader);

        // ShaderProgram cleanup
        GL.DeleteShader(vertexShader);
        if (tessControlSource != string.Empty && tessControlShader != -1)
        {
            GL.DeleteShader(tessControlShader);
        }
        if (tessEvalSource != string.Empty && tessEvalShader != -1)
        {
            GL.DeleteShader(tessEvalShader);
        }
        if (geometrySource != string.Empty && geometryShader != -1)
        {
            GL.DeleteShader(geometryShader);
        }
        GL.DeleteShader(fragmentShader);

        string infoLog = GL.GetShaderInfoLog(ProgramHandle);
        if (infoLog != string.Empty)
        {
            throw new FileLoadException($"Error compiling shader with name {name}: {infoLog}");
        }

        DebugLogger.Log($"<aqua>Successfully compiled shader <white>{name}.");
    }

    public static string GetShaderFile(string sourceName, string extension, ShaderHandler shaderHandler)
    {
        string shaderFile = Path.Combine(shaderHandler.ShaderPath, $"{sourceName}.{extension}");
        if (!File.Exists(shaderFile))
        {
            string[] files = Directory.GetFiles(shaderHandler.ShaderPath, $"{sourceName}.{extension}", SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                throw new FileNotFoundException($"Source {sourceName}.{extension} for shader {sourceName} could not be found.");
            }
            shaderFile = files[0];
        }
        return shaderFile;
    }

    public static void CompileShader(int shader, string shaderName)
    {
        if (!GL.IsShader(shader))
        {
            throw new Exception($"ShaderProgram({shader}) is not a valid shader.");
        }
        GL.CompileShader(shader);

        GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
        if (code != (int)All.True)
        {
            var infoLog = GL.GetShaderInfoLog(shader);
            throw new Exception($"Error occurred while compiling ShaderProgram({shader}) for shader {shaderName}: \n\n{infoLog}");
        }
    }

    private static void LinkProgram(int program, string shaderName)
    {
        if (!GL.IsProgram(program))
        {
            throw new Exception($"Program({program}) is not a valid program.");
        }

        GL.LinkProgram(program);

        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
        if (code != (int)All.True)
        {
            throw new Exception($"Error occurred while linking Program({program}) for shader {shaderName}: {code}");
        }

        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Program, program, $"ShaderProgram Program: {shaderName}");
    }

    private static string ProcessShaderSource(string source, string directives)
    {
        int versionIndex = source.IndexOf("#version");
        if (versionIndex == -1)
        {
            throw new Exception("ShaderProgram source does not contain #version directive.");
        }
        return source.Insert(versionIndex + 20, directives);
    }

    public void Use()
    {
        GL.UseProgram(ProgramHandle);
        GraphicsUtil.CheckError($"{Name} ShaderProgram Use");
    }

    public int GetAttribLocation(string attribName) => GL.GetAttribLocation(ProgramHandle, attribName);

    private readonly Dictionary<string, int> _uniformCache = [];
    /// <summary>Gets the uniform location for the shader program.</summary>
    public int GetUniformLocation(string uniformName)
    {
        if (_uniformCache.TryGetValue(uniformName, out int location))
        {
            return location;
        }

        location = GL.GetUniformLocation(ProgramHandle, uniformName);
        _uniformCache.Add(uniformName, location);
        return location;
    }

    /// <summary>Gets all uniform locations for the shader program.</summary>
    public List<ShaderUniform> GetAllUniformLocations()
    {
        List<ShaderUniform> uniforms = [];
        GL.GetProgram(ProgramHandle, GetProgramParameterName.ActiveUniforms, out int uniformCount);
        for (int i = 0; i < uniformCount; i++)
        {
            string name = GL.GetActiveUniform(ProgramHandle, i, out int size, out ActiveUniformType type);
            int location = GL.GetUniformLocation(ProgramHandle, name);
            ShaderUniform uniform = new()
            {
                Name = name,
                Location = location,
                Type = type,
                Size = size
            };
            uniforms.Add(uniform);
        }
        return uniforms;
    }

    public void SetInt(string name, ref int data) => GL.Uniform1(GetUniformLocation(name), data);
    public static void SetInt(int location, ref int data) => GL.Uniform1(location, data);

    public void SetInt(string name, int data) => GL.Uniform1(GetUniformLocation(name), data);
    public static void SetInt(int location, int data) => GL.Uniform1(location, data);

    public void SetFloat(string name, ref float data) => GL.Uniform1(GetUniformLocation(name), data);
    public static void SetFloat(int location, ref float data) => GL.Uniform1(location, data);

    public void SetFloat(string name, float data) => GL.Uniform1(GetUniformLocation(name), data);
    public static void SetFloat(int location, float data) => GL.Uniform1(location, data);

    public void SetBool(string name, bool data) => GL.Uniform1(GetUniformLocation(name), data ? 1 : 0);
    public static void SetBool(int location, bool data) => GL.Uniform1(location, data ? 1 : 0);

    public void SetMatrix3(string name, ref Matrix3 data) => GL.UniformMatrix3(GetUniformLocation(name), true, ref data);
    public static void SetMatrix3(int location, ref Matrix3 data) => GL.UniformMatrix3(location, true, ref data);

    public void SetMatrix4(string name, ref Matrix4 data) => GL.UniformMatrix4(GetUniformLocation(name), true, ref data);
    public static void SetMatrix4(int location, ref Matrix4 data) => GL.UniformMatrix4(location, true, ref data);

    public void SetVector2(string name, ref Vector2 data) => GL.Uniform2(GetUniformLocation(name), ref data);
    public static void SetVector2(int location, ref Vector2 data) => GL.Uniform2(location, ref data);

    public void SetVector2(string name, Vector2 data) => GL.Uniform2(GetUniformLocation(name), data);
    public static void SetVector2(int location, Vector2 data) => GL.Uniform2(location, data);

    public void SetVector3(string name, ref Vector3 data) => GL.Uniform3(GetUniformLocation(name), ref data);
    public static void SetVector3(int location, ref Vector3 data) => GL.Uniform3(location, ref data);

    public void SetVector3(string name, Vector3 data) => GL.Uniform3(GetUniformLocation(name), data);
    public static void SetVector3(int location, Vector3 data) => GL.Uniform3(location, data);

    public void SetVector4(string name, ref Vector4 data) => GL.Uniform4(GetUniformLocation(name), ref data);
    public static void SetVector4(int location, ref Vector4 data) => GL.Uniform4(location, ref data);

    public void SetVector4(string name, Vector4 data) => GL.Uniform4(GetUniformLocation(name), data);
    public static void SetVector4(int location, Vector4 data) => GL.Uniform4(location, data);

    public static implicit operator int(ShaderProgram shader) => shader.ProgramHandle;

    public void Dispose()
    {
        GL.DeleteShader(ProgramHandle);
        GC.SuppressFinalize(this);
    }
}

