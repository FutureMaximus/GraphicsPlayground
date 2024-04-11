using GraphicsPlayground.Graphics.Materials.Properties;
using GraphicsPlayground.Graphics.Models;
using GraphicsPlayground.Graphics.Render;
using GraphicsPlayground.Graphics.Shaders;

namespace GraphicsPlayground.Graphics.Materials;

/// <summary>A renderable material that can be applied to a mesh.</summary>
public abstract class Material(string name)
{
    /// <summary>The properties of the material.</summary>
    public List<MaterialProperty> Properties = [];
    /// <summary> The name of the material. </summary>
    public string Name = name;
    /// <summary> Whether the material has been modified. </summary>
    public bool Modified;
    /// <summary> The shader program associated with this material. </summary>'
    public ShaderProgram? ShaderProgram;
    /// <summary> Uses the material and passes a reference of the material being used. </summary>
    public abstract void Use(in IMesh mesh);
    /// <summary> Whether the material has been built. </summary>
    public bool HasBeenBuilt = false;
    /// <summary> The shading model of the material. </summary>
    public MaterialShadingModel ShadingModel = MaterialShadingModel.DefaultLit;
    /// <summary> The name of the shader program to look for when building the material. </summary>
    public abstract string ShaderProgramName { get; }
}
