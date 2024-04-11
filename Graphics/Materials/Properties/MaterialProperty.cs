using GraphicsPlayground.Graphics.Shaders;

namespace GraphicsPlayground.Graphics.Materials.Properties;

/// <summary> A single property for a material. </summary>
public abstract class MaterialProperty(string uniformName)
{
    /// <summary> Uses the material property in a shader program. </summary>
    public abstract void UseMaterialProperty(ref ShaderProgram shaderProgram);

    /// <summary> The type name of the material property. </summary>
    public abstract string TypeName { get; }

    /// <summary> The name of the uniform used in the shader. </summary>
    public string UniformName = uniformName;

    /// <summary> Whether this material should be updated every frame. </summary>
    public bool ShouldUpdate = true;
}
