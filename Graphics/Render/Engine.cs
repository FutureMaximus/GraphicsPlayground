using GraphicsPlayground.Util;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Concurrent;
using GraphicsPlayground.Graphics.Textures;
using OpenTK.Windowing.Desktop;
using GraphicsPlayground.Graphics.Shaders;
using GraphicsPlayground.Graphics.Shaders.Data;
using GraphicsPlayground.Graphics.Render.RenderPasses.SubPasses;
using GraphicsPlayground.Graphics.Lighting.Lights;
using GraphicsPlayground.Graphics.Lighting;
using GraphicsPlayground.Scripts;
using OpenTK.Windowing.Common;
using GraphicsPlayground.Graphics.Terrain.Meshing;
using GraphicsPlayground.Graphics.Models;
using System.Reflection;

namespace GraphicsPlayground.Graphics.Render;

public class Engine
{
    /// <summary> Window that owns this engine. </summary>
    public GameWindow Window { get; set; }
    /// <summary> Handles shaders in the engine. </summary>
    public ShaderHandler? ShaderHandler;
    public Camera Camera { get; } = new();
    public Matrix4 Projection;
    public Matrix4 ClusteredRenderProjection;
    public Matrix4 Orthographic;
    public Settings EngineSettings;
    public readonly List<IRenderPass> RenderPasses = [];
    public readonly List<Model> Models = [];
    public readonly List<TerrainMesh> TerrainMeshes = []; // TODO: Move to models.
    /// <summary> Streamed assets that may have tasks to be ran on the main thread </summary>
    public readonly ConcurrentStack<IAssetHolder> StreamedAssets = new();
    public readonly AssetStreamer AssetStreamer;
    public ScreenFBO? Screen;
    public List<IScript> Scripts { get; } = [];
    public List<Light> Lights;
    public DirectionalLight? DirectionalLight;
    public Action? OnCustomImGuiLogic;
    public Dictionary<MethodInfo, object> CustomUIStuff = [];

    public float DeltaTime { get; set; }
    public float TimeElapsed
    {
        get => _timeElapsed;
        set
        {
            _timeElapsed = value;
            if (_timeElapsed >= float.MaxValue)
            {
                _timeElapsed = 0.0f;
            }
        }
    }
    private float _timeElapsed = 0.0f;
    public float FPS { get; set; }

    public Engine(GameWindow window)
    {
        Window = window;
        EngineSettings = new()
        {
            UseDeferredRendering = false,
            UseClusteredForwardRendering = true,
            UseForwardRendering = true,
            UseDebugRendering = false,
            UseOrthographic = false,
            MaximumLights = 7,
            FieldOfView = 70f,
            AspectRatio = 1f,
            DepthNear = 0.1f,
            DepthFar = 25000f,
            ClusteredDepthNear = 0.1f,
            ClusteredDepthFar = 25000f,
            ClearColor = [0.05f, 0.05f, 0.5f, 1.0f]
        };
        Lights = new(EngineSettings.MaximumLights);
        AssetStreamer = new(this);
        DeltaTime = 0.0f;
        TimeElapsed = 0.0f;
        Random rand = new();
        for (int i = 0; i < EngineSettings.MaximumLights; i++)
        {
            Vector3 randColor = new((float)rand.NextDouble(),(float)rand.NextDouble(),(float)rand.NextDouble());
            PBRLightData newLightData = new()
            {
                Color = randColor,
                Intensity = 5f,
                Range = 500f,
                Constant = 1.0f,
                Linear = 0.09f,
                Quadratic = 0.032f,
                Enabled = false
            };
            float range = 100;
            Vector3 randLoc = new(
                              (float)rand.NextDouble() * range - 50,
                              (float)rand.NextDouble() * range - 50,
                              (float)rand.NextDouble() * range - 50);
            PointLight newLight = new(randLoc, newLightData);
            Lights.Add(newLight);
        }
        PBRLightData lightData = new()
        {
            Color = new Vector3(1.0f, 1.0f, 1.0f),
            Intensity = 10.0f,
        };
        DirectionalLight directionalLight = new(new Vector3(0.5f, 1.0f, 0.0f), lightData);
        DirectionalLight = directionalLight;
    }

    public struct Settings
    {
        public bool UseDeferredRendering;
        public bool UseForwardRendering;
        public bool UseClusteredForwardRendering;
        public bool UseDebugRendering;
        public bool UseOrthographic;
        public int MaximumLights { get; set; }
        public float FieldOfView;
        public Vector2i WindowSize;
        public Vector2i WindowPosition;
        public float AspectRatio;
        public float DepthNear;
        public float DepthFar;
        public float ClusteredDepthNear;
        public float ClusteredDepthFar;
        public float[] ClearColor;
    }

    /// <summary>
    /// Loads the engine and its components.
    /// This should be called after the window is created.
    /// </summary>
    public void Load()
    {
        if (Window is null)
        {
            throw new Exception("Window is null.");
        }
        foreach (Model model in Models)
        {
            foreach (ModelPart modelPart in model.Parts)
            {
                foreach (IMesh mesh in modelPart.Meshes)
                {
                    if (mesh.Material is not null)
                    {
                        mesh.Material.Build(this);
                        mesh.Material.HasBeenBuilt = true;
                    }
                    mesh.Load();
                }
            }
        }
        Window.Resize += Window_Resize;
        GraphicsUtil.LoadDebugger();
        ScriptLoader.LoadAllScripts(this);
        ForwardRendering forwardRendering = new(this)
        {
            IsEnabled = EngineSettings.UseForwardRendering
        };
        ClusteredForwardRendering clusteredForwardRendering = new(this)
        {
            IsEnabled = EngineSettings.UseClusteredForwardRendering
        };
        RenderPasses.Add(forwardRendering);
        // If a texture is non-existing or invalid, use this instead.
        TextureEntries.AddTexture(TextureHelper.GenerateTextureNotFound());
        GL.ClearColor(
            EngineSettings.ClearColor[0],
            EngineSettings.ClearColor[1],
            EngineSettings.ClearColor[2],
            EngineSettings.ClearColor[3]
            );
        EngineSettings.AspectRatio = Window.ClientSize.X / (float)Window.ClientSize.Y;
        EngineSettings.WindowSize = Window.ClientSize;
        EngineSettings.WindowPosition = new(0, 0);
        if (EngineSettings.UseOrthographic)
        {
            Projection = Matrix4.CreateOrthographicOffCenter(
                -EngineSettings.AspectRatio,
                 EngineSettings.AspectRatio,
                 -1.0f,
                 1.0f,
                 EngineSettings.DepthNear,
                 EngineSettings.DepthFar
                 );
            Orthographic = Projection;
            ClusteredRenderProjection = Matrix4.CreateOrthographicOffCenter(
                -EngineSettings.AspectRatio,
                 EngineSettings.AspectRatio,
                 -1.0f,
                 1.0f,
                 EngineSettings.ClusteredDepthNear,
                 EngineSettings.ClusteredDepthFar
                 );
        }
        else
        {
            Projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(EngineSettings.FieldOfView),
                EngineSettings.AspectRatio,
                EngineSettings.DepthNear,
                EngineSettings.DepthFar
                );
            ClusteredRenderProjection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(EngineSettings.FieldOfView),
                EngineSettings.AspectRatio,
                EngineSettings.DepthNear,
                EngineSettings.DepthFar
                );
        }
        ShaderHandler = new(Config.Settings.ShaderPath);
        ShaderProgram screenFBOShader = new(ShaderHandler, "ScreenFBO", "screen");
        Screen = new(screenFBOShader, Window.ClientSize);
        Screen.Load();
        GlobalShaderData.LoadBuffers(this);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        foreach (IRenderPass renderPass in RenderPasses)
        {
            if (renderPass.IsEnabled)
            {
                renderPass.Load();
            }
        }
    }

    private void Window_Resize(ResizeEventArgs args)
    {
        EngineSettings.WindowSize = args.Size;
        EngineSettings.AspectRatio = args.Size.X / (float)args.Size.Y;
        Screen?.Resize(args.Size);
    }

    public void Render()
    {
        // ============= Update =============
        if (EngineSettings.UseOrthographic)
        {
            Projection = Matrix4.CreateOrthographicOffCenter(
                -EngineSettings.AspectRatio,
                 EngineSettings.AspectRatio,
                 -1.0f,
                 1.0f,
                 EngineSettings.DepthNear,
                 EngineSettings.DepthFar
                 );
            Orthographic = Projection;
        }
        else
        {
            Projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(EngineSettings.FieldOfView),
                EngineSettings.AspectRatio,
                EngineSettings.DepthNear,
                EngineSettings.DepthFar
                );
        }
        
        EngineImGuiHelper.Update(this);
        OnCustomImGuiLogic?.Invoke();

        if (Window is not null)
        {
            EngineSettings.WindowSize = Window.Size;
            EngineSettings.AspectRatio = Window.Size.X / (float)Window.Size.Y;
        }
        // ==================================

        // ============= UBO (Global ShaderProgram Data) =============
        ProjViewUniform projViewUniform = new(Projection, Camera.View, Camera.Position);
        GlobalShaderData.UpdateProjViewUBO(ref projViewUniform);
        // ====================================================

        // ============= Render ==============
        Screen?.Bind();
        GL.ClearColor(
            EngineSettings.ClearColor[0],
            EngineSettings.ClearColor[1],
            EngineSettings.ClearColor[2],
            EngineSettings.ClearColor[3]
            );
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        GL.Enable(EnableCap.DepthTest);
        //GL.Enable(EnableCap.CullFace);
        //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

        foreach (IRenderPass renderPass in RenderPasses)
        {
            if (renderPass.IsEnabled)
            {
                renderPass.Render();
            }
        }

        //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.Disable(EnableCap.DepthTest);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        Screen?.Render();
        // ===================================
    }

    public void Update(float deltaTime)
    {
        DeltaTime = deltaTime;
        TimeElapsed += deltaTime;

        foreach (IScript script in Scripts)
        {
            if (script.ShouldUpdate)
            {
                script.Update();
            }
        }

        List<IAssetHolder> streamedAssets = StreamedAssets.ToList() ?? [];
        foreach (IAssetHolder asset in streamedAssets)
        {
            if (asset is AssetStreamer.StreamingAsset streamingAsset)
            {
                if (streamingAsset.Loaded)
                {
                    byte[]? bytes = streamingAsset.Data;
                    if (bytes is null)
                    {
                        RemoveAsset();
                        continue;
                    }
                    streamingAsset.AfterLoadedExecute(bytes, streamingAsset.AssetObjectData);
                    RemoveAsset();
                    continue;
                }
            }
            else if (asset is AssetStreamer.StreamingAssetPackage streamingAssetPackage)
            {
                if (streamingAssetPackage.Loaded)
                {
                    byte[][]? bytes = streamingAssetPackage.Data;
                    if (bytes is null)
                    {
                        RemoveAsset();
                        continue;
                    }
                    streamingAssetPackage.AfterLoadedExecute(bytes, streamingAssetPackage.AssetObjectData);
                    RemoveAsset();
                    continue;
                }
            }
            RemoveAsset();
        }
    }

    private void RemoveAsset()
    {
        if (StreamedAssets.TryPop(out IAssetHolder? poppedAsset))
        {
            poppedAsset.Dispose();
        }
    }

    public void ShutDown()
    {
        foreach (IScript script in Scripts)
        {
            script.OnUnload();
        }
        foreach (IRenderPass renderPass in RenderPasses)
        {
            renderPass.Dispose();
        }
        ShaderHandler?.Dispose();
        GlobalShaderData.Dispose();
        TextureEntries.Dispose();
    }
}
