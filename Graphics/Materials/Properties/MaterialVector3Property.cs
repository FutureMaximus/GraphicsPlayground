using GraphicsPlayground.Graphics.Shaders;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Materials.Properties;

public class MaterialVector3Property(string name, Vector3 value) : MaterialProperty(name)
{
    public Vector3 Value = value;

    public override void UseMaterialProperty(ref ShaderProgram shaderProgram)
    {
        if (!ShouldUpdate) return;
        shaderProgram.SetVector3(UniformName, Value);
    }

    public override string TypeName => "Vector3";
}
