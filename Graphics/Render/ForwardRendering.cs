using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using GraphicsPlayground.Graphics.Lighting;
using GraphicsPlayground.Graphics.Lighting.Lights;
using GraphicsPlayground.Graphics.Models;
using GraphicsPlayground.Graphics.Shaders.Data;
using GraphicsPlayground.Graphics.Textures;
using GraphicsPlayground.Graphics.Models.Generic;
using GraphicsPlayground.Graphics.Terrain.Meshing;
using GraphicsPlayground.Graphics.Models.Mesh;

namespace GraphicsPlayground.Graphics.Render;

/// <summary> Forward rendering for generic models. </summary>
public class ForwardRendering : IRenderPass
{
    /// <summary> Reference to the engine. </summary>
    public Engine Engine;
    /// <summary> WorldSettings of shadow rendering. </summary>
    public ShadowInternalSettings ShadowSettings;

    #region Shaders

    private Shader? _pbrShader;
    private Shader? _terrainShader;
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
        _pbrShader = new(Engine.ShaderHandler, "PBR", "pbr");
        _terrainShader = new(Engine.ShaderHandler, "Terrain", "terrain");
    }
    #endregion

    #region Render
    public void Render()
    {
        // ============ PBR pass =============
        if (_pbrShader is null)
        {
            throw new NullReferenceException("PBR shader is not set.");
        }
        if (Engine.DirectionalLight is null)
        {
            throw new NullReferenceException("Directional light is not set.");
        }
        _pbrShader.Use();
        _pbrShader.SetVector3($"dirLight.direction", ref Engine.DirectionalLight.Position);
        _pbrShader.SetVector3($"dirLight.color", ref Engine.DirectionalLight.LightData.Color);
        _pbrShader.SetFloat($"dirLight.intensity", ref Engine.DirectionalLight.LightData.Intensity);
        int pntLightI = 0;
        for (int i = 0; i < Engine.EngineSettings.MaximumLights; i++)
        {
            Light light = Engine.Lights[i];
            if (light is PointLight pbrPointLight)
            {
                _pbrShader.SetVector3($"pointLights[{pntLightI}].position", ref pbrPointLight.Position);
                _pbrShader.SetVector3($"pointLights[{pntLightI}].color", ref pbrPointLight.LightData.Color);
                _pbrShader.SetFloat($"pointLights[{pntLightI}].intensity", ref pbrPointLight.LightData.Intensity);
                _pbrShader.SetFloat($"pointLights[{pntLightI}].constant", ref pbrPointLight.LightData.Constant);
                _pbrShader.SetFloat($"pointLights[{pntLightI}].linear", ref pbrPointLight.LightData.Linear);
                _pbrShader.SetFloat($"pointLights[{pntLightI}].quadratic", ref pbrPointLight.LightData.Quadratic);
                pntLightI++;
            }
        }

        foreach (GenericModel model in Engine.Models)
        {
            ModelRenderData modelRenderData = model.ModelRenderData;
            if (!modelRenderData.Visible) continue;
            _pbrShader.SetBool("shadowEnabled", modelRenderData.ShadowEnabled);

            foreach (GenericModelPart corePart in model.Parts)
            {
                Matrix4 meshTransform = corePart.Transformation;
                _pbrShader.SetMatrix4("model", ref meshTransform);
                Matrix3 normalMat = corePart.NormalMatrix();
                _pbrShader.SetMatrix3("normalMatrix", ref normalMat);
                foreach (GenericMesh mesh in corePart.Meshes)
                {
                    if (!mesh.IsLoaded)
                    {
                        mesh.Load();
                        continue;
                    }
                    _pbrShader.SetBool("hasTangents", mesh.HasTangents);

                    GenericMeshShaderData shaderData = mesh.ShaderData;
                    PBRMaterialData pbrMat = shaderData.MaterialData;
                    Texture2D albedo = pbrMat.AlbedoTexture;
                    Texture2D normal = pbrMat.NormalTexture;
                    Texture2D arm = pbrMat.ARMTexture;
                    if (!albedo.Loaded) continue;
                    albedo?.Use(TextureUnit.Texture1);
                    _pbrShader.SetInt("material.albedoMap", 1);
                    if (!normal.Loaded) continue;
                    normal?.Use(TextureUnit.Texture2);
                    _pbrShader.SetInt("material.normalMap", 2);
                    if (!arm.Loaded) continue;
                    arm?.Use(TextureUnit.Texture3);
                    _pbrShader.SetInt("material.ARMMap", 3);

                    mesh.Render();
                }
            }
        }
        foreach (TerrainMesh terrainMesh in Engine.TerrainMeshes)
        {
            _terrainShader?.Use();
            Matrix4 translation = terrainMesh.Translation;
            Matrix4 scale = Matrix4.CreateScale(1f);
            Matrix4 identity = Matrix4.Identity;
            Matrix3 identity3 = Matrix3.Identity;
            //_terrainShader?.SetMatrix4("model", ref translation); TODO: Implement way for each chunk to have its position matrix instead of just using identity.
            Matrix4 modelMat = scale;
            Matrix4 test = scale * translation;
            _terrainShader?.SetMatrix4("model", ref test);
            _terrainShader?.SetMatrix3("normalMatrix", ref identity3);
            terrainMesh.Render();
        }
        // ======================================
    }
    #endregion

    public void Dispose()
    {
        _pbrShader?.Dispose();
        GC.SuppressFinalize(this);
    }
}
