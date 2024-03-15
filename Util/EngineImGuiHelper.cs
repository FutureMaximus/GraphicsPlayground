using GraphicsPlayground.Graphics.Render;
using GraphicsPlayground.Scripts;
using ImGuiNET;

namespace GraphicsPlayground.Util;

public static class EngineImGuiHelper
{
    public static void Update(Engine engine)
    {
        ImGui.Begin("Engine");
        ImGui.BeginChild("Scripts", new System.Numerics.Vector2(0, 0), true);
        if (ImGui.Button("Reload All Scripts"))
        {
            ScriptLoader.LoadAllScripts(engine);
        }
        foreach (IScript script in engine.Scripts)
        {
            ImGui.Text(script.GetType().Name);
            ImGui.SameLine();
            if (ImGui.Button("Run"))
            {
                script.OnReload();
            }
        }
        ImGui.EndChild();
        ImGui.End();
    }
}
