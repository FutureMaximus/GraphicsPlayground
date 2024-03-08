﻿using GraphicsPlayground.Graphics.Render;
using ImGuiNET;

namespace GraphicsPlayground.Util;

public static class EngineImGuiHelper
{
    public static void Update(Engine engine)
    {
        ImGui.Begin("Engine");
        ImGui.Text($"FPS: {engine.FPS}");
        ImGui.Text($"Camera Position: {engine.Camera.Position}");
        ImGui.End();
    }
}
