using GraphicsPlayground.Graphics.Materials;
using GraphicsPlayground.Graphics.Models;
using GraphicsPlayground.Graphics.Models.Generic;
using GraphicsPlayground.Graphics.Models.Mesh;
using GraphicsPlayground.Graphics.Models.ShapeModels;
using GraphicsPlayground.Graphics.Render;
using GraphicsPlayground.Graphics.Shaders.Data;
using GraphicsPlayground.Graphics.Terrain.World;
using GraphicsPlayground.Graphics.Textures;
using GraphicsPlayground.Util;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Drawing;

namespace GraphicsPlayground.Scripts.InternalScripts;

public class TestScript : IScript
{
    public VoxelWorld? World;
    public Engine? Engine;

    void IScript.OnLoad(Engine engine)
    {
        Model model = new("model");
        ModelPart spherePart = new("Sphere Part", model)
        {
            LocalTransformation = new()
            {
                Position = new(0f, 0f, 20f),
                Scale = new(1f, 1f, 1f)
            }
        };
        ModelPart donutPart = new("Donut Part", model)
        {
            LocalTransformation = new()
            {
                Position = new(50f, 0f, 0f),
                Scale = new(1f, 1f, 1f)
            }
        };
        Texture2D defaultAlbedo = TextureHelper.GenerateColorTexture(Color.Blue, 128, 128);
        Texture2D metallic = TextureHelper.GenerateColorTexture(Color.White, 128, 128);
        Texture2D defaultNormal = TextureHelper.GenerateColorTexture(ColorHelper.DefaultNormalMapColor, 128, 128);
        Texture2D defaultARM = TextureHelper.GenerateColorTexture(Color.FromArgb(255, 100, 200), 128, 128);
        GenericMesh sphereMesh = new Sphere(spherePart, 10, 50, 100)
        {
            Material = new PBRMaterial("Sphere PBR Material")
            {
                Albedo = defaultAlbedo,
                Normal = defaultNormal,
                Metallic = metallic,
                Roughness = 0.5f,
                AmbientOcclusion = 1.0f
            }
        };
        GenericMesh donutMesh = new Torus(donutPart, 20, 5, 50, 100)
        {
            Material = new PBRMaterial("Donut PBR Material")
            {
                Albedo = new Vector3(1.0f, 0.5f, 0.5f),
                Normal = defaultNormal,
                Metallic = metallic,
                Roughness = 0.8f,
                AmbientOcclusion = 0.5f
            }
        };
        spherePart.Meshes.Add(sphereMesh);
        donutPart.Meshes.Add(donutMesh);
        model.Parts.Add(spherePart);
        model.Parts.Add(donutPart);
        engine.Meshes.Add(sphereMesh);
        engine.Meshes.Add(donutMesh);

        WorldSettings settings = new()
        {
            TargetPosition = new Vector3(0, 0, 0),
            WorldSize = CoordinateUtilities.NextPowerOf2(500)
        };
        Noise heightNoise = new();
        heightNoise.SetNoiseType(Noise.NoiseType.OpenSimplex2S);
        heightNoise.SetFractalLacunarity(2);
        heightNoise.SetFrequency(0.05f);
        heightNoise.SetDomainWarpAmp(2);
        heightNoise.SetFractalOctaves(8);
        Noise densityNoise = new();
        densityNoise.SetNoiseType(Noise.NoiseType.Value);
        Noise caveNoise = new();
        densityNoise.SetNoiseType(Noise.NoiseType.OpenSimplex2);
        //caveNoise.SetDomainWarpAmp(10);*/
        GeneratorSettings generatorSettings = new()
        {
            HeightmapNoise = heightNoise,
            DensityNoise = densityNoise,
            CaveNoise = caveNoise,
        };
        VoxelWorld world = new(engine, settings, generatorSettings);
        World = world;
        Engine = engine;
        World = world;
        /*World?.Start();
        World?.Update();*/
        //LODGenerator.GenerateLODs(sphereMesh, 12, 15000, 200);

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
        Console.WriteLine("Reloaded!");
    }

    static RateLimiter rateLimiter = new(1);

    void CustomImGui()
    {
        /*ImGui.Begin("Terrain LOD");
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
                //World?.ExtractMesh(LOD);
            }
        }
        ImGui.Text($"Meshes: {Engine.TerrainMeshes.Count}");
        ImGui.Text($"FPS: {Engine.FPS}");
        ImGui.SetWindowFontScale(1f);
        ImGui.End();*/
    }

    bool IScript.ShouldUpdate => true;

    void IScript.Update()
    {
        //World?.Update();
    }
}
