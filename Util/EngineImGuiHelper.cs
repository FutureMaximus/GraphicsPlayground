using GraphicsPlayground.Graphics.Materials;
using GraphicsPlayground.Graphics.Materials.Properties;
using GraphicsPlayground.Graphics.Models;
using GraphicsPlayground.Graphics.Render;
using GraphicsPlayground.Scripts;
using ImGuiNET;

namespace GraphicsPlayground.Util;

public static class EngineImGuiHelper
{
    public const float WindowFontScale = 2f;

    public static void Update(Engine engine)
    {
        ImGui.Begin("Graphics Playground");
        ImGui.SetWindowFontScale(WindowFontScale);

        ImGui.BeginChild("Engine", new System.Numerics.Vector2(0, 0), true);
        ImGui.Text("Engine Info");
        ImGui.Separator();
        ImGui.Text($"FPS: {engine.FPS}");
        ImGui.Text($"Delta Time: {engine.DeltaTime}");
        ImGui.Text($"Meshes: {engine.Meshes.Count}");
        ImGui.Spacing();
        ImGui.Spacing();

        ImGui.Text("Content");
        ImGui.SameLine();
        if (ImGui.Button("Add Mesh"))
        {
            // TODO: Add mesh
        }
        ImGui.SameLine();
        if (ImGui.Button("Add Material"))
        {
            // TODO: Add material
        }
        ImGui.BeginChild("Content", new System.Numerics.Vector2(0, 0), true);
        ImGui.SetWindowFontScale(WindowFontScale);
        if (ImGui.CollapsingHeader("Meshes"))
        {
            foreach (IMesh mesh in engine.Meshes)
            {
                ImGui.Text(mesh.Name);
                ImGui.Separator();
                ImGui.Text($"Vertices: {mesh.Vertices.Count}");
                ImGui.Text($"Indices: {mesh.Indices.Count}");
                ImGui.Text($"Material: {mesh.Material?.Name}");
                ImGui.Spacing();
                ImGui.Spacing();
            }
        }
        if (ImGui.CollapsingHeader("Materials"))
        {
            List<Material> foundMaterials = [];
            foreach (IMesh mesh in engine.Meshes)
            {
                if (mesh.Material != null && !foundMaterials.Contains(mesh.Material))
                {
                    foundMaterials.Add(mesh.Material);
                }
            }
            foreach (Material material in foundMaterials)
            {
                ImGui.Text(material.Name);
                ImGui.Separator();
                foreach (MaterialProperty property in material.Properties)
                {
                    ImGui.Text($"Uniform Name: {property.UniformName}");
                    ImGui.Text($"Type: {property.TypeName}");
                    ImGui.Spacing();
                    ImGui.Spacing();
                }
                ImGui.Spacing();
                ImGui.Spacing();
            }
        }
        ImGui.EndChild();

        ImGui.Text("Scripts");
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
