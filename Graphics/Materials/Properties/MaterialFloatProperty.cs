﻿using GraphicsPlayground.Graphics.Shaders;

namespace GraphicsPlayground.Graphics.Materials.Properties;

/// <summary> A float property for a material. </summary>
public class MaterialFloatProperty(string uniformName, float value) : MaterialProperty(uniformName)
{
    public float Value = value;

    public override void UseMaterialProperty(ref ShaderProgram shaderProgram)
    {
        if (!ShouldUpdate) return;
        shaderProgram.SetFloat(UniformName, Value);
    }

    public override string TypeName => "float";
}
