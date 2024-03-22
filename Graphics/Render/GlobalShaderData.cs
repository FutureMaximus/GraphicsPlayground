using GraphicsPlayground.Graphics.Lighting;
using GraphicsPlayground.Graphics.Lighting.Lights;
using GraphicsPlayground.Graphics.Shaders.Data;
using GraphicsPlayground.Util;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace GraphicsPlayground.Graphics.Render;

/// <summary>Global shader data that can be accessed from any shader.</summary>
public static class GlobalShaderData
{
    /// <summary>Uniform buffer object that contains the projection, view, and view position.</summary>
    public static int ProjViewUBO { get; private set; }
    /// <summary>ShaderProgram storage buffer object that contains the cluster data.</summary>
    public static int ClusterSSBO { get; private set; }
    /// <summary>ShaderProgram storage buffer object that contains the screen to view data.</summary>
    public static int Screen2ViewSSBO { get; private set; }
    /// <summary>ShaderProgram storage buffer object that contains the light data.</summary>
    public static int LightDataSSBO { get; private set; }
    /// <summary>The light index list SSBO.</summary>
    public static int LightIndexListSSBO { get; private set; }
    /// <summary>The light grid SSBO.</summary>
    public static int LightGridSSBO { get; private set; }
    /// <summary>Light index global count SSBO.</summary>
    public static int LightIndexGlobalCountSSBO { get; private set; }

    public static readonly uint GRID_SIZE_X = 16;
    public static readonly uint GRID_SIZE_Y = 9;
    public static readonly uint GRID_SIZE_Z = 24;
    public static readonly uint GRID_SIZE = GRID_SIZE_X * GRID_SIZE_Y * GRID_SIZE_Z;
    public const uint MAX_LIGHTS_PER_CLUSTER = 50;

    public static void LoadBuffers(Engine engine)
    {
        // Uniform Buffer Objects (Read-Only)

        // ProjView UBO
        ProjViewUBO = GL.GenBuffer();
        // 64 bytes for projection matrix, 64 bytes for view matrix, 16 bytes for camera position.
        GL.BindBuffer(BufferTarget.UniformBuffer, ProjViewUBO);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, ProjViewUBO, "ProjViewUBO Location 0");
        int projViewUBOSize = Marshal.SizeOf(typeof(Matrix4)) * 2 + Marshal.SizeOf(typeof(Vector3));
        GL.BufferData(BufferTarget.UniformBuffer, projViewUBOSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        GraphicsUtil.CheckError("UBO 0 (ProjView) Buffer Data");

        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, ProjViewUBO);
        GraphicsUtil.CheckError("UBO 0 (ProjView) Buffer Base");

        ProjViewUniform projViewUniform = new(engine.Projection, engine.Camera.View, engine.Camera.Position);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, Marshal.SizeOf(typeof(Matrix4)), ref projViewUniform.Projection);
        GL.BufferSubData(BufferTarget.UniformBuffer, Marshal.SizeOf(typeof(Matrix4)), Marshal.SizeOf(typeof(Matrix4)), ref projViewUniform.View);
        GL.BufferSubData(BufferTarget.UniformBuffer, Marshal.SizeOf(typeof(Matrix4)) * 2, Marshal.SizeOf(typeof(Vector3)), ref projViewUniform.ViewPos);
        GraphicsUtil.CheckError("UBO 0 (ProjView) Error");

        GL.BindBuffer(BufferTarget.UniformBuffer, 0);

        // ShaderProgram Storage Buffer Objects (Read-Write)

        // Cluster Data SSBO
        // 4 bytes for min location (x, y, z) and 4 bytes for max location (x, y, z).
        ClusterSSBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ClusterSSBO);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, sizeof(float) * 8 * (int)GRID_SIZE, IntPtr.Zero, BufferUsageHint.StaticCopy);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, ClusterSSBO, "ClusterSSBO Location 1");
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, ClusterSSBO);
        GraphicsUtil.CheckError("SSBO 1 (Cluster) Buffer Base");
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

        // Screen2View SSBO
        Screen2View screen2View;
        screen2View.InverseProjection = Matrix4.Invert(engine.ClusteredRenderProjection);
        screen2View.TileSizeX = GRID_SIZE_X;
        screen2View.TileSizeY = GRID_SIZE_Y;
        screen2View.TileSizeZ = GRID_SIZE_Z;
        screen2View.TileSizePixels.X = 1f / MathF.Ceiling(engine.Window.ClientSize.X / (float)GRID_SIZE_X);
        screen2View.TileSizePixels.Y = 1f / MathF.Ceiling(engine.Window.ClientSize.Y / (float)GRID_SIZE_Y);
        screen2View.ViewPixelSize = new Vector2(1f / engine.Window.ClientSize.X, 1f / engine.Window.ClientSize.Y);
        // Basically reduced a log function into a simple multiplication an addition by pre-calculating these
        screen2View.SliceScalingFactor = GRID_SIZE_Z / MathF.Log2(engine.EngineSettings.ClusteredDepthFar / engine.EngineSettings.ClusteredDepthNear);
        screen2View.SliceBiasFactor = -(GRID_SIZE_Z * MathF.Log2(
            engine.EngineSettings.ClusteredDepthNear) / MathF.Log2(engine.EngineSettings.ClusteredDepthFar / engine.EngineSettings.ClusteredDepthNear));
        Screen2ViewSSBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Screen2ViewSSBO);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, Marshal.SizeOf(typeof(Screen2View)), IntPtr.Zero, BufferUsageHint.StaticCopy);
        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, Marshal.SizeOf(typeof(Screen2View)), ref screen2View);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, Screen2ViewSSBO, "Screen2ViewSSBO Location 2");
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, Screen2ViewSSBO);
        GraphicsUtil.CheckError("SSBO 2 (Screen2View) Buffer Base");
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

        // Light Data SSBO
        // 12 bytes for position, 4 bytes for max range.
        LightDataSSBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, LightDataSSBO);
        GL.BufferData(BufferTarget.ShaderStorageBuffer,
            Marshal.SizeOf(typeof(GPUPointLightData)) * engine.EngineSettings.MaximumLights, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, LightDataSSBO, "LightDataSSBO Location 3");
        int pointLightIndex = 0;
        for (int i = 0; i < engine.EngineSettings.MaximumLights; i++)
        {
            Light light = engine.Lights[i];
            if (light is PointLight pointLight)
            {
                PBRLightData lightData = pointLight.LightData;
                GPUPointLightData gpuPointLightData = new()
                {
                    Position = pointLight.Position,
                    MaxRange = lightData.MaxRange,
                    Color = lightData.Color,
                    Intensity = lightData.Intensity,
                    Constant = lightData.Constant,
                    Linear = lightData.Linear,
                    Quadratic = lightData.Quadratic
                };
                // TODO: Implement light updates.
                GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 
                    Marshal.SizeOf(typeof(GPUPointLightData)) * pointLightIndex,
                    Marshal.SizeOf(typeof(GPUPointLightData)), ref gpuPointLightData);
                GraphicsUtil.CheckError("SSBO 3 (LightData) Buffer Sub Data");
                pointLightIndex++;
            }
        }
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, LightDataSSBO);
        GraphicsUtil.CheckError("SSBO 3 (LightData) Buffer Base");
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

        // Light Index List SSBO
        uint numberOfLights = GRID_SIZE * MAX_LIGHTS_PER_CLUSTER;
        LightIndexListSSBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, LightIndexListSSBO);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, sizeof(uint) * (int)numberOfLights, IntPtr.Zero, BufferUsageHint.StaticCopy);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, LightIndexListSSBO, "LightIndexListSSBO Location 4");
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 4, LightIndexListSSBO);
        GraphicsUtil.CheckError("SSBO 4 (LightIndexList) Buffer Base");
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

        // Light Grid SSBO
        // Every tile takes two unsigned ints one to represent the number of lights in that grid.
        // Another to represent the offset to the light index list from where to begin reading light indexes from.
        // This implementation is from the Olsson paper.
        LightGridSSBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, LightGridSSBO);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, sizeof(uint) * 2 * (int)GRID_SIZE, IntPtr.Zero, BufferUsageHint.StaticCopy);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, LightGridSSBO, "LightGridSSBO Location 5");
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 5, LightGridSSBO);
        GraphicsUtil.CheckError("SSBO 5 (LightGrid) Buffer Base");
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

        // Light Index Global Count SSBO
        LightIndexGlobalCountSSBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, LightIndexGlobalCountSSBO);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, sizeof(uint), IntPtr.Zero, BufferUsageHint.StaticCopy);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, LightIndexGlobalCountSSBO, "LightIndexGlobalCountSSBO Location 6");
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 6, LightIndexGlobalCountSSBO);
        GraphicsUtil.CheckError("SSBO 6 (LightIndexGlobalCount) Buffer Base");
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

        GraphicsUtil.CheckError("Global ShaderProgram Data Buffer Init");
    }

    /// <summary>Updates the ProjView UBO with the new projection, view, and view position.</summary>
    public static void UpdateProjViewUBO(ref ProjViewUniform projViewUniform)
    {
        GL.BindBuffer(BufferTarget.UniformBuffer, ProjViewUBO);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, Marshal.SizeOf(typeof(Matrix4)), ref projViewUniform.Projection);
        GL.BufferSubData(BufferTarget.UniformBuffer, Marshal.SizeOf(typeof(Matrix4)), Marshal.SizeOf(typeof(Matrix4)), ref projViewUniform.View);
        GL.BufferSubData(BufferTarget.UniformBuffer, Marshal.SizeOf(typeof(Matrix4)) * 2, Marshal.SizeOf(typeof(Vector3)), ref projViewUniform.ViewPos);
        GraphicsUtil.CheckError("UBO 0 (ProjView) Error");
        GL.BindBuffer(BufferTarget.UniformBuffer, 0);
    }

    // TODO: Update Light Data if there are changes to the lights.
    public static void Dispose()
    {
        GL.DeleteBuffer(ProjViewUBO);
        GL.DeleteBuffer(ClusterSSBO);
        GL.DeleteBuffer(Screen2ViewSSBO);
        GL.DeleteBuffer(LightDataSSBO);
    }
}
