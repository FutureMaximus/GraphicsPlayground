using GraphicsPlayground.Graphics.Materials.Properties;
using GraphicsPlayground.Graphics.Models;
using GraphicsPlayground.Graphics.Render;
using GraphicsPlayground.Graphics.Shaders;
using GraphicsPlayground.Graphics.Textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Text;

namespace GraphicsPlayground.Graphics.Materials;

///<summary> A standard physically based rendering material for opaque meshes. </summary>
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

    public override string ShaderProgramName => "pbr_cluster";

    public override void Use(in IMesh mesh)
    {
        if (ShaderProgram == null)
        {
            throw new Exception("ParentMaterial must be built and have a shader before use.");
        }
        ShaderProgram.Use();
        foreach (MaterialProperty property in Properties)
        {
            property.UseMaterialProperty(ref ShaderProgram);
        }
        ShaderProgram.SetBool("hasTangents", mesh.HasTangents);
        Matrix4 model = mesh.ParentPart.Transformation;
        ShaderProgram.SetMatrix4("model", ref model);
        Matrix3 normal = mesh.ParentPart.NormalMatrix();
        ShaderProgram.SetMatrix3("normalMatrix", ref normal);
    }

    /// <summary> Processes the material and builds the shader directives and properties. </summary>
    public static void Process(PBRMaterial pbrMaterial, List<string> directives, StringBuilder sb)
    {
        if (pbrMaterial.ARM != null)
        {
            directives.Add("#define USING_ARM_MAP");
            pbrMaterial.Properties.Add(new MaterialTextureProperty("material.armMap", pbrMaterial.ARM, TextureUnit.Texture4));
        }
        else
        {
            if (pbrMaterial.Albedo == null)
            {
                directives.Add("#define ALBEDO_VECTOR3");
                pbrMaterial.Properties.Add(new MaterialVector3Property("material.albedo", new Vector3(0)));
            }
            else if (pbrMaterial.Albedo.GetType() == typeof(Texture2D))
            {
                pbrMaterial.Properties.Add(new MaterialTextureProperty("material.albedoMap", (Texture2D)pbrMaterial.Albedo, TextureUnit.Texture0));
            }
            else if (pbrMaterial.Albedo.GetType() == typeof(Vector3))
            {
                directives.Add("#define ALBEDO_VECTOR3");
                pbrMaterial.Properties.Add(new MaterialVector3Property("material.albedo", (Vector3)pbrMaterial.Albedo));
            }
            else
            {
                throw new Exception("Albedo must be of type Texture2D or Vector3.");
            }

            if (pbrMaterial.Normal != null)
            {
                pbrMaterial.Properties.Add(new MaterialTextureProperty("material.normalMap", pbrMaterial.Normal, TextureUnit.Texture1));
            }
            else
            {
                pbrMaterial.Properties.Add(new MaterialTextureProperty("material.normalMap", TextureHelper.GenerateNormalTexture(), TextureUnit.Texture1));
            }

            if (pbrMaterial.Metallic == null)
            {
                directives.Add("#define METALLIC_FLOAT");
                pbrMaterial.Properties.Add(new MaterialFloatProperty("material.metallic", 0f));
            }
            else if (pbrMaterial.Metallic.GetType() == typeof(Texture2D))
            {
                pbrMaterial.Properties.Add(new MaterialTextureProperty("material.metallic", (Texture2D)pbrMaterial.Metallic, TextureUnit.Texture2));
            }
            else if (pbrMaterial.Metallic.GetType() == typeof(float))
            {
                directives.Add("#define METALLIC_FLOAT");
                pbrMaterial.Properties.Add(new MaterialFloatProperty("material.metallic", (float)pbrMaterial.Metallic));
            }
            else
            {
                throw new Exception("Metallic must be of type Texture2D or float.");
            }

            if (pbrMaterial.Roughness == null)
            {
                directives.Add("#define ROUGHNESS_FLOAT");
                pbrMaterial.Properties.Add(new MaterialFloatProperty("material.roughness", 0f));
            }
            else if (pbrMaterial.Roughness.GetType() == typeof(Texture2D))
            {
                pbrMaterial.Properties.Add(new MaterialTextureProperty("material.roughness", (Texture2D)pbrMaterial.Roughness, TextureUnit.Texture3));
            }
            else if (pbrMaterial.Roughness.GetType() == typeof(float))
            {
                directives.Add("#define ROUGHNESS_FLOAT");
                pbrMaterial.Properties.Add(new MaterialFloatProperty("material.roughness", (float)pbrMaterial.Roughness));
            }
            else
            {
                throw new Exception("Roughness must be of type Texture2D or float.");
            }

            if (pbrMaterial.AmbientOcclusion == null)
            {
                directives.Add("#define AMBIENT_OCCLUSION_FLOAT");
                pbrMaterial.Properties.Add(new MaterialFloatProperty("material.ambientOcclusion", 0f));
            }
            else if (pbrMaterial.AmbientOcclusion.GetType() == typeof(Texture2D))
            {
                pbrMaterial.Properties.Add(new MaterialTextureProperty("material.ambientOcclusion", (Texture2D)pbrMaterial.AmbientOcclusion, TextureUnit.Texture4));
            }
            else if (pbrMaterial.AmbientOcclusion.GetType() == typeof(float))
            {
                directives.Add("#define AMBIENT_OCCLUSION_FLOAT");
                pbrMaterial.Properties.Add(new MaterialFloatProperty("material.ambientOcclusion", (float)pbrMaterial.AmbientOcclusion));
            }
            else
            {
                throw new Exception("AmbientOcclusion must be of type Texture2D or float.");
            }
        }

        foreach (string directive in directives)
        {
            sb.Append($"{directive}\n");
        }
    }
}
