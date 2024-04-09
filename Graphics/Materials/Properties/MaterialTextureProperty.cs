using GraphicsPlayground.Graphics.Shaders;
using GraphicsPlayground.Graphics.Textures;
using OpenTK.Graphics.OpenGL4;

namespace GraphicsPlayground.Graphics.Materials.Properties;

///<summary> A texture property for a material. </summary>
public class MaterialTextureProperty(string name, Texture2D texture, TextureUnit textureUnit) : MaterialProperty(name)
{
    public Texture2D Texture = texture;
    public TextureUnit TextureUnit = textureUnit;
    public int TextureLocation = textureUnit - TextureUnit.Texture0;

    public override void UseMaterialProperty(ref ShaderProgram shaderProgram) 
    {
        Texture.Use(TextureUnit);
        shaderProgram.SetInt(UniformName, TextureLocation);
    }

    public override string TypeName => "Texture";
}
