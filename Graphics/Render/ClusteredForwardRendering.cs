using GraphicsPlayground.Graphics.Lighting;
using GraphicsPlayground.Graphics.Lighting.Lights;
using GraphicsPlayground.Graphics.Materials;
using GraphicsPlayground.Graphics.Models;
using GraphicsPlayground.Graphics.Shaders;
using OpenTK.Graphics.OpenGL4;

namespace GraphicsPlayground.Graphics.Render;

public class ClusteredForwardRendering : IRenderPass
{
    public bool IsEnabled { get; set; }
    public Engine Engine;
    public ComputeShader? AABBShader;
    public ComputeShader? LightCullShader;

    public ClusteredForwardRendering(Engine engine)
    {
        Engine = engine;
        if (Engine.ShaderHandler == null)
        {
            return;
        }
        AABBShader = new ComputeShader(Engine.ShaderHandler, "ClusterAABB", "clusterAABB");
        LightCullShader = new ComputeShader(Engine.ShaderHandler, "Cluster", "clusterCullLight");
    }

    /// <summary>Loads the renderer.</summary>
    public void Load()
    {
        if (AABBShader == null || LightCullShader == null)
        {
            return;
        }
        AABBShader.Use();
        ShaderProgram.SetFloat(0, Engine.EngineSettings.ClusteredDepthNear);
        ShaderProgram.SetFloat(1, Engine.EngineSettings.ClusteredDepthFar);
        GL.DispatchCompute(GlobalShaderData.GRID_SIZE_X, GlobalShaderData.GRID_SIZE_Y, GlobalShaderData.GRID_SIZE_Z);
    }

    /// <summary>Updates the renderer.</summary>
    public void Render()
    {
        if (AABBShader == null || LightCullShader == null)
        {
            return;
        }
        // This should be moved somewhere else that updates when needed but for now it's fine
        /*AABBShader.Use();
        ShaderProgram.SetFloat(0, Engine.EngineSettings.ClusteredDepthNear);
        ShaderProgram.SetFloat(1, Engine.EngineSettings.ClusteredDepthFar);
        GL.DispatchCompute(GlobalShaderData.GRID_SIZE_X, GlobalShaderData.GRID_SIZE_Y, GlobalShaderData.GRID_SIZE_Z);*/
        LightCullShader.Use();
        GL.DispatchCompute(1, 1, 6);
        foreach (IMesh mesh in Engine.Meshes)
        {
            if (!mesh.IsLoaded)
            {
                mesh.Load();
                continue;
            }
            if (mesh.Material is null) continue;
            if (!mesh.Material.HasBeenBuilt)
            {
                mesh.Material.Build(Engine);
                mesh.Material.HasBeenBuilt = true;
            }
            mesh.Material.Use(mesh);
            if (mesh.Material.ShadingModel == MaterialShadingModel.DefaultLit && mesh.Material.ShaderProgram != null)
            {
                mesh.Material.ShaderProgram.SetFloat("zNear", Engine.EngineSettings.ClusteredDepthNear);
                mesh.Material.ShaderProgram.SetFloat("zFar", Engine.EngineSettings.ClusteredDepthFar);
                mesh.Material.ShaderProgram.SetVector3("dirLight.direction", ref Engine.DirectionalLight.Position); // TODO: Add to global light data
                mesh.Material.ShaderProgram.SetVector3("dirLight.color", ref Engine.DirectionalLight.LightData.Color);
                mesh.Material.ShaderProgram.SetFloat("dirLight.intensity", 2f);
            }
            mesh.Render();
        }
    }

    public void Dispose()
    {
        AABBShader?.Dispose();
        LightCullShader?.Dispose();
        GC.SuppressFinalize(this);
    }
}
