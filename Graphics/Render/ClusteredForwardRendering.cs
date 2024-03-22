using GraphicsPlayground.Graphics.Shaders;
using OpenTK.Graphics.OpenGL4;

namespace GraphicsPlayground.Graphics.Render;

public class ClusteredForwardRendering : IRenderPass
{
    public bool IsEnabled { get; set; }
    public Engine Engine;
    public ComputeShader? AABBShader;
    public ComputeShader? ClusterShader;

    public ClusteredForwardRendering(Engine engine)
    {
        Engine = engine;
        if (Engine.ShaderHandler == null)
        {
            return;
        }
        AABBShader = new ComputeShader(Engine.ShaderHandler, "ClusterAABB", "clusterAABB");
        ClusterShader = new ComputeShader(Engine.ShaderHandler, "Cluster", "cluster");
    }

    public void Load()
    {
        if (AABBShader == null || ClusterShader == null)
        {
            return;
        }
        AABBShader.Use();
        ShaderProgram.SetFloat(0, Engine.EngineSettings.ClusteredDepthNear);
        ShaderProgram.SetFloat(1, Engine.EngineSettings.ClusteredDepthFar);
        GL.DispatchCompute(GlobalShaderData.GRID_SIZE_X, GlobalShaderData.GRID_SIZE_Y, GlobalShaderData.GRID_SIZE_Z);
    }

    public void Render()
    {
        // This should be moved somewhere else but for now it's fine
        AABBShader?.Use();
        ShaderProgram.SetFloat(0, Engine.EngineSettings.ClusteredDepthNear);
        ShaderProgram.SetFloat(1, Engine.EngineSettings.ClusteredDepthFar);
        GL.DispatchCompute(GlobalShaderData.GRID_SIZE_X, GlobalShaderData.GRID_SIZE_Y, GlobalShaderData.GRID_SIZE_Z);

    }

    public void Dispose()
    {
        AABBShader?.Dispose();
        ClusterShader?.Dispose();
        GC.SuppressFinalize(this);
    }
}
