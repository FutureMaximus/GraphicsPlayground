using GraphicsPlayground.Graphics.Models.Generic;
using GraphicsPlayground.Graphics.Models.ShapeModels;
using GraphicsPlayground.Graphics.Render;
using GraphicsPlayground.Graphics.Shaders.Data;
using GraphicsPlayground.Graphics.Terrain;
using GraphicsPlayground.Graphics.Terrain.World;
using GraphicsPlayground.Graphics.Textures;
using GraphicsPlayground.Util;
using ImGuiNET;
using OpenTK.Mathematics;
using System.Drawing;

namespace GraphicsPlayground.Scripts.InternalScripts;

public class TestScript : IScript
{
    public VoxelWorld? World;
    public Engine Engine;

    void IScript.OnLoad(Engine engine)
    {
        GenericModel sphere = new("sphere");
        GenericModelPart spherePart = new("Sphere Part", sphere)
        {
            LocalTransformation = new()
            {
                Position = new(0f, 0f, 20f),
                Scale = new(1f, 1f, 1f)
            }
        };
        Texture2D defaultAlbedo = TextureHelper.GenerateColorTexture(Color.White, 128, 128);
        Texture2D defaultNormal = TextureHelper.GenerateColorTexture(ColorHelper.DefaultNormalMapColor, 128, 128);
        Texture2D defaultARM = TextureHelper.GenerateColorTexture(Color.FromArgb(255, 100, 200), 128, 128);
        GenericMesh sphereMesh = new Torus(spherePart, 10, 5, 50, 100)
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

        VoxelWorld world = new(engine, 200);
        world.TestGenerate();
        world.ExtractMesh(LOD);
        World = world;
        Engine = engine;

        engine.OnCustomImGuiLogic += CustomImGui;
    }

    static int LOD = 1;
    static int Volumesize = 128;

    void IScript.OnUnload()
    {
        Console.WriteLine("Unloaded TestScript");
    }

    void IScript.OnReload()
    {
        Console.WriteLine("TestScript OnReload");
    }

    static RateLimiter rateLimiter = new(1);

    void CustomImGui()
    {
        ImGui.Begin("Terrain LOD");
        ImGui.SetWindowFontScale(2f);
        if (ImGui.SliderInt("LOD", ref LOD, 1, 12))
        {
            if (rateLimiter.CanProceed(Engine.TimeElapsed))
            {
                foreach (TerrainMesh mesh in Engine.TerrainMeshes)
                {
                    mesh.Dispose();
                }
                Engine.TerrainMeshes.Clear();
                World?.ExtractMesh(LOD);
            }
        }
        ImGui.Text($"Meshes: {Engine.TerrainMeshes.Count}");
        ImGui.Text($"FPS: {Engine.FPS}");
        ImGui.SetWindowFontScale(1f);
        ImGui.End();
    }

    bool IScript.ShouldUpdate => true;

    void IScript.Update()
    {
        ImGui.Begin("Test Script");
        ImGui.Text("Test Script");
        ImGui.End();
    }
}
