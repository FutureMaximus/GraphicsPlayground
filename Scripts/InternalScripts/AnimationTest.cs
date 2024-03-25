using GraphicsPlayground.Graphics.Models;
using GraphicsPlayground.Graphics.Render;

namespace GraphicsPlayground.Scripts.InternalScripts;

internal class AnimationTest : IScript
{
    void IScript.OnLoad(Engine engine)
    {
        ModelLoader.ModelEntry entry = new()
        {
            Name = "AnimationTest",
            Path = "C:\\BlenderModels\\AnimationTest",
            ModelFile = "AnimationTest.obj",
            TexturePath = "Models/AnimationTest/AnimationTest.png"
        };
        ModelLoader.ProcessModel(entry, engine.AssetStreamer);
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
