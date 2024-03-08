﻿using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using GraphicsPlayground.Graphics.Lighting;
using GraphicsPlayground.Graphics.Lighting.Lights;
using GraphicsPlayground.Graphics.Models;
using GraphicsPlayground.Graphics.Shaders;
using GraphicsPlayground.Graphics.Shaders.Data;
using GraphicsPlayground.Graphics.Textures;
using GraphicsPlayground.Graphics.Models.Generic;

namespace GraphicsPlayground.Graphics.Render;

/// <summary> Forward rendering for generic models. </summary>
public class ForwardRendering : IRenderPass
{
    /// <summary> Reference to the engine. </summary>
    public Engine Engine;
    /// <summary> Settings of shadow rendering. </summary>
    public ShadowInternalSettings ShadowSettings;

    #region Shaders

    /// <summary> The PBR shader used for rendering models. </summary>
    /// <param name="engine"></param>
    private Shader? _pbrShader;
    #endregion

    private DirectionalLight? DirectionalLight;

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

        // ======= PBR loading ========
        _pbrShader = new(Engine.ShaderHandler, "PBR", "pbr");
        for (int i = 0; i < Engine.EngineSettings.MaximumLights; i++)
        {
            Light light = Engine.Lights[i];
            if (light is DirectionalLight pbrDirectionalLight)
            {
                DirectionalLight = pbrDirectionalLight;
            }
        }
        // =============================
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
        _pbrShader.Use();
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
            else if (light is DirectionalLight pbrDirectionalLight)
            {
                _pbrShader.SetVector3($"dirLight.direction", ref pbrDirectionalLight.Position);
                _pbrShader.SetVector3($"dirLight.color", ref pbrDirectionalLight.LightData.Color);
                _pbrShader.SetFloat($"dirLight.intensity", ref pbrDirectionalLight.LightData.Intensity);
            }
        }

        foreach (GenericModel model in Engine.GenericModels)
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
        // ======================================
    }
    #endregion

    public void Dispose()
    {
        _pbrShader?.Dispose();
        GC.SuppressFinalize(this);
    }
}
