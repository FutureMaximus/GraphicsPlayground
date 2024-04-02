using GraphicsPlayground.Graphics.Materials.Properties;
using GraphicsPlayground.Graphics.Models;
using GraphicsPlayground.Graphics.Render;
using GraphicsPlayground.Graphics.Shaders;
using GraphicsPlayground.Graphics.Textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Text;

namespace GraphicsPlayground.Graphics.Materials;

///<summary> A standard physically based rendering material. </summary>
public class PBRMaterial(string name) : Material(name)
{
    ///<summary> 
    ///The albedo color of the material.
    ///<para>Possible types: Texture2D, Vector3 Defaults to 0,0,0</para>
    ///</summary>
    public object? Albedo;
    ///<summary> The normal map of the material. </summary>
    public Texture2D? Normal;
    ///<summary> 
    ///The metallic value of the material. 
    ///<para>Possible types: Texture2D, float. Defaults to 0.</para>
    ///</summary>
    public object? Metallic;
    ///<summary>
    ///The roughness value of the material.
    ///<para>Possible types: Texture2D, float. Defaults to 0.</para>
    ///</summary>
    public object? Roughness;
    ///<summary>
    ///The ambient occlusion value of the material.
    ///<para>Possible types: Texture2D, float. Defaults to 0.</para>
    ///</summary>
    public object? AmbientOcclusion;
    ///<summary>
    ///The ARM map (ambient occlusion = r, roughness = g, metallic = b) 
    ///of the material if utilized metallic, roughness, and ambient occlusion will be ignored.
    ///</summary>
    public Texture2D? ARM;

    public override void Use(in IMesh mesh)
    {
        if (ShaderProgram == null)
        {
            throw new Exception("Material must be built and have a shader before use.");
        }
        ShaderProgram.Use();
        foreach (MaterialProperty property in Properties)
        {
            property.UseMaterialProperty(ref ShaderProgram);
        }
        if (mesh.HasTangents)
        {
            ShaderProgram.SetBool("hasTangents", true);
        }
        else
        {
            ShaderProgram.SetBool("hasTangents", false);
        }
        Matrix4 model = mesh.ParentPart.Transformation;
        ShaderProgram.SetMatrix4("model", ref model);
        Matrix3 normal = mesh.ParentPart.NormalMatrix();
        ShaderProgram.SetMatrix3("normalMatrix", ref normal);
    }

    public override void Build(Engine engine)
    {
        if (engine.ShaderHandler == null)
        {
            throw new Exception("Engine must have a shader handler to build materials.");
        }

        List<string> directives = [];

        if (ARM != null)
        {
            directives.Add("#define USING_ARM_MAP");
            Properties.Add(new MaterialTextureProperty("material.armMap", ARM, TextureUnit.Texture4));
        }
        else
        {
            if (Albedo == null)
            {
                directives.Add("#define ALBEDO_VECTOR3");
                Properties.Add(new MaterialVector3Property("material.albedo", new Vector3(0)));
            }
            else if (Albedo.GetType() == typeof(Texture2D))
            {
                Properties.Add(new MaterialTextureProperty("material.albedoMap", (Texture2D)Albedo, TextureUnit.Texture0));
            }
            else if (Albedo.GetType() == typeof(Vector3))
            {
                directives.Add("#define ALBEDO_VECTOR3");
                Properties.Add(new MaterialVector3Property("material.albedo", (Vector3)Albedo));
            }
            else
            {
                throw new Exception("Albedo must be of type Texture2D or Vector3.");
            }

            if (Normal != null)
            {
                Properties.Add(new MaterialTextureProperty("material.normalMap", Normal, TextureUnit.Texture1));
            }
            else
            {
                Properties.Add(new MaterialTextureProperty("material.normalMap", TextureHelper.GenerateNormalTexture(), TextureUnit.Texture1));
            }

            if (Metallic == null)
            {
                directives.Add("#define METALLIC_FLOAT");
                Properties.Add(new MaterialFloatProperty("material.metallic", 0f));
            }
            else if (Metallic.GetType() == typeof(Texture2D))
            {
                Properties.Add(new MaterialTextureProperty("material.metallic", (Texture2D)Metallic, TextureUnit.Texture2));
            }
            else if (Metallic.GetType() == typeof(float))
            {
                directives.Add("#define METALLIC_FLOAT");
                Properties.Add(new MaterialFloatProperty("material.metallic", (float)Metallic));
            }
            else
            {
                throw new Exception("Metallic must be of type Texture2D or float.");
            }

            if (Roughness == null)
            {
                directives.Add("#define ROUGHNESS_FLOAT");
                Properties.Add(new MaterialFloatProperty("material.roughness", 0f));
            }
            else if (Roughness.GetType() == typeof(Texture2D))
            {
                Properties.Add(new MaterialTextureProperty("material.roughness", (Texture2D)Roughness, TextureUnit.Texture3));
            }
            else if (Roughness.GetType() == typeof(float))
            {
                directives.Add("#define ROUGHNESS_FLOAT");
                Properties.Add(new MaterialFloatProperty("material.roughness", (float)Roughness));
            }
            else
            {
                throw new Exception("Roughness must be of type Texture2D or float.");
            }

            if (AmbientOcclusion == null)
            {
                directives.Add("#define AMBIENT_OCCLUSION_FLOAT");
                Properties.Add(new MaterialFloatProperty("material.ambientOcclusion", 0f));
            }
            else if (AmbientOcclusion.GetType() == typeof(Texture2D))
            {
                Properties.Add(new MaterialTextureProperty("material.ambientOcclusion", (Texture2D)AmbientOcclusion, TextureUnit.Texture4));
            }
            else if (AmbientOcclusion.GetType() == typeof(float))
            {
                directives.Add("#define AMBIENT_OCCLUSION_FLOAT");
                Properties.Add(new MaterialFloatProperty("material.ambientOcclusion", (float)AmbientOcclusion));
            }
            else
            {
                throw new Exception("AmbientOcclusion must be of type Texture2D or float.");
            }
        }

        StringBuilder sb = new();
        foreach (string directive in directives)
        {
            sb.Append($"{directive}\n");
        }

        ShaderProgram = new ShaderProgram(engine.ShaderHandler, Name, "pbr", sb.ToString());
    }
}
