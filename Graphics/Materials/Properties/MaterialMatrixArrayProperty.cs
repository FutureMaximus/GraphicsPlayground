using GraphicsPlayground.Graphics.Shaders;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Materials.Properties;

public class MaterialMatrixArrayProperty(string uniformName, List<Matrix4> matrixArray) : MaterialProperty(uniformName)
{
    public override string TypeName => "Material Matrix Array Property";
    public List<Matrix4> Value = matrixArray;

    public override void UseMaterialProperty(ref ShaderProgram shaderProgram)
    {
        if (!ShouldUpdate) return;
        for (int i = 0; i < Value.Count; i++)
        {
            Matrix4 mat = Value[i];
            shaderProgram.SetMatrix4($"{UniformName}[{i}]", ref mat);
        }
    }
}
