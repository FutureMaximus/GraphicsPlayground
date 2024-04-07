using GraphicsPlayground.Graphics.Lighting;
using GraphicsPlayground.Graphics.Lighting.Lights;
using GraphicsPlayground.Graphics.Shaders.Data;
using GraphicsPlayground.Util;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
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
    public static readonly uint GRID_SIZE_Y = 8;
    public static readonly uint GRID_SIZE_Z = 24;
    public static readonly uint GRID_SIZE = GRID_SIZE_X * GRID_SIZE_Y * GRID_SIZE_Z;
    public const uint MAX_LIGHTS_PER_CLUSTER = 100;

    public static void LoadBuffers(Engine engine)
    {
        // Uniform Buffer Objects (Read-Only)

        // ProjView UBO
        // 64 bytes for projection matrix, 64 bytes for view matrix, 16 bytes for camera position.
        ProjViewUBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.UniformBuffer, ProjViewUBO);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, ProjViewUBO, "ProjViewUBO Location 0");
        int projViewUBOSize = Unsafe.SizeOf<Matrix4>() * 2 + Unsafe.SizeOf<Vector3>();
        GL.BufferData(BufferTarget.UniformBuffer, projViewUBOSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        GraphicsUtil.CheckError("UBO 0 (ProjView) Buffer Data");
        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, ProjViewUBO);
        GraphicsUtil.CheckError("UBO 0 (ProjView) Buffer Base");
        ProjViewUniform projViewUniform = new(engine.Projection, engine.Camera.View, engine.Camera.Position);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, Unsafe.SizeOf<Matrix4>(), ref projViewUniform.Projection);
        GL.BufferSubData(BufferTarget.UniformBuffer, Unsafe.SizeOf<Matrix4>(), Unsafe.SizeOf<Matrix4>(), ref projViewUniform.View);
        GL.BufferSubData(BufferTarget.UniformBuffer, Unsafe.SizeOf<Matrix4>() * 2, Unsafe.SizeOf<Vector3>(), ref projViewUniform.ViewPos);
        GraphicsUtil.CheckError("UBO 0 (ProjView) Error");
        GL.BindBuffer(BufferTarget.UniformBuffer, 0);

        // Shader Storage Buffer Objects (Read-Write)

        // Cluster Data SSBO
        // 32 bytes for min and max AABB * (GRID_SIZE_X * GRID_SIZE_Y * GRID_SIZE_Z) number of bytes.
        ClusterSSBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ClusterSSBO);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, (int)(GRID_SIZE * Unsafe.SizeOf<VolumeTileAABB>()), IntPtr.Zero, BufferUsageHint.StaticCopy);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, ClusterSSBO, "ClusterSSBO Location 1");
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, ClusterSSBO);
        GraphicsUtil.CheckError("SSBO 1 (Cluster) Buffer Base");
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

        // Screen2View SSBO
        // 64 bytes for inverse projection, 16 bytes for tile sizes, 8 bytes for screen size, 4 bytes for slice scaling factor, 4 bytes for slice bias factor.
        int sizeX = (int)MathF.Ceiling(engine.Window.ClientSize.X / (float)GRID_SIZE_X);
        Screen2View screen2View;
        screen2View.InverseProjection = engine.Projection.Inverted();
        screen2View.TileSizes = new Vector4i((int)GRID_SIZE_X, (int)GRID_SIZE_Y, (int)GRID_SIZE_Z, sizeX);
        screen2View.ScreenSize = new Vector2i(engine.Window.ClientSize.X, engine.Window.ClientSize.Y);
        float depthFar = engine.EngineSettings.DepthFar;
        float depthNear = engine.EngineSettings.DepthNear;
        screen2View.SliceScalingFactor = GRID_SIZE_Z / MathF.Log2(depthFar / depthNear);
        screen2View.SliceBiasFactor = -(GRID_SIZE_Z * MathF.Log2(depthNear) / MathF.Log2(depthFar / depthNear));
        Screen2ViewSSBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Screen2ViewSSBO);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, Unsafe.SizeOf<Screen2View>(), ref screen2View, BufferUsageHint.StaticCopy);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, Screen2ViewSSBO, "Screen2View SSBO Location 2");
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, Screen2ViewSSBO);
        GraphicsUtil.CheckError("SSBO 2 (Screen2View) Buffer Base");
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

        // Light Data SSBO
        // 12 bytes for position, 4 bytes for range * (The maximum number of lights) number of bytes.
        LightDataSSBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, LightDataSSBO);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, Unsafe.SizeOf<GPUPointLightData>() * engine.EngineSettings.MaximumLights, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, LightDataSSBO, "LightDataSSBO Location 3");
        IntPtr lightBufferPtr = GL.MapBuffer(BufferTarget.ShaderStorageBuffer, BufferAccess.WriteOnly);
        if (lightBufferPtr == IntPtr.Zero)
        {
            throw new Exception("Failed to map LightDataSSBO buffer.");
        }
        GraphicsUtil.CheckError("SSBO 3 (LightData) Buffer Map");
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
                    Range = lightData.Range,
                    Color = lightData.Color
                };
                Marshal.StructureToPtr(gpuPointLightData, lightBufferPtr + Marshal.SizeOf(typeof(GPUPointLightData)) * pointLightIndex, false);
                pointLightIndex++;
            }
        }
        GL.UnmapBuffer(BufferTarget.ShaderStorageBuffer);
        GraphicsUtil.CheckError("SSBO 3 (LightData) Buffer Unmap");
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, LightDataSSBO);
        GraphicsUtil.CheckError("SSBO 3 (LightData) Bind Buffer Base");
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

        // Light Index List SSBO
        uint numberOfLights = GRID_SIZE * MAX_LIGHTS_PER_CLUSTER;
        LightIndexListSSBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, LightIndexListSSBO);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, sizeof(uint) * (int)numberOfLights, IntPtr.Zero, BufferUsageHint.StaticCopy);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, LightIndexListSSBO, "LightIndexListSSBO Location 4");
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 4, LightIndexListSSBO);
        GraphicsUtil.CheckError("SSBO 4 (LightIndexList) Bind Buffer Base");
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
        GraphicsUtil.CheckError("SSBO 5 (LightGrid) Bind Buffer Base");
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

        // Light Index Global Count SSBO
        LightIndexGlobalCountSSBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, LightIndexGlobalCountSSBO);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, sizeof(uint), IntPtr.Zero, BufferUsageHint.StaticCopy);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, LightIndexGlobalCountSSBO, "LightIndexGlobalCountSSBO Location 6");
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 6, LightIndexGlobalCountSSBO);
        GraphicsUtil.CheckError("SSBO 6 (LightIndexGlobalCount) Bind Buffer Base");
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

        GraphicsUtil.CheckError("Global ShaderProgram Data Buffer Init");
    }

    /// <summary>Updates the ProjView UBO with the new projection, view, and view position.</summary>
    public static void UpdateProjViewUBO(ref ProjViewUniform projViewUniform)
    {
        GL.BindBuffer(BufferTarget.UniformBuffer, ProjViewUBO);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, Unsafe.SizeOf<Matrix4>(), ref projViewUniform.Projection);
        GL.BufferSubData(BufferTarget.UniformBuffer, Unsafe.SizeOf<Matrix4>(), Unsafe.SizeOf<Matrix4>(), ref projViewUniform.View);
        GL.BufferSubData(BufferTarget.UniformBuffer, Unsafe.SizeOf<Matrix4>() * 2, Unsafe.SizeOf<Vector3>(), ref projViewUniform.ViewPos);
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
