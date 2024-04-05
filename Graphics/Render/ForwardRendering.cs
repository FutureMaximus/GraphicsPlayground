using OpenTK.Mathematics;
using GraphicsPlayground.Graphics.Lighting;
using GraphicsPlayground.Graphics.Lighting.Lights;
using GraphicsPlayground.Graphics.Models;
using GraphicsPlayground.Graphics.Terrain.Meshing;
using GraphicsPlayground.Graphics.Shaders;
using GraphicsPlayground.Graphics.Materials;

namespace GraphicsPlayground.Graphics.Render;

/// <summary> Forward rendering for generic models. </summary>
public class ForwardRendering : IRenderPass
{
    /// <summary> Reference to the engine. </summary>
    public Engine Engine;
    /// <summary> WorldSettings of shadow rendering. </summary>
    public ShadowInternalSettings ShadowSettings;

    #region Shaders
    private ShaderProgram? _terrainShader;
    #endregion

    public ForwardRendering(Engine engine)
    {
        Engine = engine;
        ShadowSettings = new()
        {
            DepthMapResolution = 4096,
            LightProjectionTuning = -10.0f,
            ShadowStartDepthNear = 500f,
            ShadowEndDepthFar = 2000.0f,
            ShadowCascadeLevels =
            [
                750,
                1000,
                1500,
                1750,
            ]
        };
    }

    #region Settings
    public struct ShadowInternalSettings
    {
        /// <summary> Resolution of the shadow map. </summary>
        public int DepthMapResolution;
        /// <summary> The depth at which the shadow map cascades start. </summary>
        public float ShadowStartDepthNear;
        /// <summary> The depth at which the shadow map cascades end. </summary>
        public float ShadowEndDepthFar;
        /// <summary> 
        /// Tune this value to adjust the size of the light projection. Higher values will result in a larger projection. 
        /// </summary>
        public float LightProjectionTuning;
        /// <summary>
        /// Shadow cascade levels this is used to determine the distance of each shadow cascade
        /// where the first level is the closest offering higher resolution 
        /// and the last level is the farthest offering lower resolution.
        /// </summary>
        public float[] ShadowCascadeLevels;
    }
    #endregion

    public bool IsEnabled { get; set; }

    #region Loading
    public void Load()
    {
        if (Engine.ShaderHandler is null) return;
        _terrainShader = new(Engine.ShaderHandler, "Terrain", "terrain");
    }
    #endregion

    #region Render
    public void Render()
    {
        if (Engine.DirectionalLight is null)
        {
            throw new NullReferenceException("Directional light is not set.");
        }
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
            // TODO: Use SSBO for light data
            if (mesh.Material.ShadingModel == MaterialShadingModel.DefaultLit && mesh.Material.ShaderProgram != null)
            {
                mesh.Material.ShaderProgram.SetVector3("dirLight.direction", ref Engine.DirectionalLight.Position);
                mesh.Material.ShaderProgram.SetVector3("dirLight.color", ref Engine.DirectionalLight.LightData.Color);
                mesh.Material.ShaderProgram.SetFloat("dirLight.intensity", ref Engine.DirectionalLight.LightData.Intensity);
                int pntLightI = 0;
                for (int i = 0; i < Engine.EngineSettings.MaximumLights; i++)
                {
                    Light light = Engine.Lights[i];
                    if (light is PointLight pbrPointLight)
                    {
                        mesh.Material.ShaderProgram.SetVector3($"pointLights[{pntLightI}].position", ref pbrPointLight.Position);
                        mesh.Material.ShaderProgram.SetVector3($"pointLights[{pntLightI}].color", ref pbrPointLight.LightData.Color);
                        mesh.Material.ShaderProgram.SetFloat($"pointLights[{pntLightI}].intensity", ref pbrPointLight.LightData.Intensity);
                        mesh.Material.ShaderProgram.SetFloat($"pointLights[{pntLightI}].range", ref pbrPointLight.LightData.Range);
                        pntLightI++;
                    }
                }
            }
            mesh.Render();
        }
        foreach (TerrainMesh terrainMesh in Engine.TerrainMeshes) // TODO: Add this mesh to engine.
        {
            _terrainShader?.Use();
            Matrix4 translation = terrainMesh.Translation;
            Matrix3 identityNorm = Matrix3.Identity;
            _terrainShader?.SetMatrix4("model", ref translation);
            _terrainShader?.SetMatrix3("normalMatrix", ref identityNorm);
            terrainMesh.Render();
        }
    }
    #endregion

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
