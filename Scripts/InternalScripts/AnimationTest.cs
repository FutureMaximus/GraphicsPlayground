using GraphicsPlayground.Graphics.Materials;
using GraphicsPlayground.Graphics.Render;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Scripts.InternalScripts;

internal class AnimationTest : IScript
{
    void IScript.OnLoad(Engine engine)
    {
        /*Material pbrMat = new PBRMaterial("TestPBR")
        {
            Albedo = new Vector3(1.0f, 0.0f, 0.0f),
            Metallic = 0.0f,
            Roughness = 0.0f,
        };*/

        /*ModelLoader.ModelEntry entry = new()
        {
            Name = "AnimationTest",
            Path = "C:\\BlenderModels\\AnimationTest",
            ModelFile = "AnimationTest.obj",
            TexturePath = "Models/AnimationTest/AnimationTest.png"
        };
        ModelLoader.ProcessModel(entry, engine.AssetStreamer);*/
    }

    void IScript.OnReload()
    {
    }

    void IScript.OnUnload()
    {
    }

    public bool ShouldUpdate = false;

    void IScript.Update()
    {
    }
}
