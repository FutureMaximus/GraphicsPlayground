using GraphicsPlayground.Graphics.Models.Generic;
using GraphicsPlayground.Graphics.Models.ShapeModels;
using GraphicsPlayground.Graphics.Render;
using GraphicsPlayground.Graphics.Shaders.Data;
using GraphicsPlayground.Graphics.Textures;
using GraphicsPlayground.Util;
using System.Drawing;

namespace GraphicsPlayground.Scripts.InternalScripts;

public class TestScript : IScript
{
    void IScript.OnLoad(Engine engine)
    {
        GenericModel sphere = new("sphere");
        GenericModelPart spherePart = new("Sphere Part", sphere)
        {
            LocalTransformation = new()
            {
                Position = new(0f, -10f, 0f),
                Scale = new(10f, 10f, 10f)
            }
        };
        Texture2D defaultAlbedo = TextureHelper.GenerateColorTexture(Color.Gray, 128, 128);
        Texture2D defaultNormal = TextureHelper.GenerateColorTexture(ColorHelper.DefaultNormalMapColor, 128, 128);
        Texture2D defaultARM = TextureHelper.GenerateColorTexture(Color.FromArgb(255, 100, 200), 128, 128);
        GenericMesh sphereMesh = new Sphere(spherePart, 1, 20, 50)
        {
            ShaderData = new GenericMeshShaderData(
                new PBRMaterialData()
                {
                    AlbedoTexture = defaultAlbedo,
                    NormalTexture = defaultNormal,
                    ARMTexture = defaultARM,
                }
            )
        };
        spherePart.Meshes.Add(sphereMesh);
        sphere.Parts.Add(spherePart);
        engine.GenericModels.Add(sphere);
    }

    void IScript.OnUnload()
    {
        Console.WriteLine("Unloaded TestScript");
    }

    void IScript.Run()
    {
        Console.WriteLine("TestScript Run");
    }

    bool IScript.ShouldUpdate => false;

    void IScript.Update()
    {
        Console.WriteLine("TestScript Update");
    }
}
