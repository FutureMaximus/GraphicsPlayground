using System.Reflection;
using GraphicsPlayground.Graphics.Render;

namespace GraphicsPlayground.Util;

/// <summary>Loads content to be used in the engine.</summary>
public static class ContentHelper
{
    public static string ContentPath { get; private set; } = string.Empty;

    public static void LoadContent(Engine engine)
    {
        string? assemblyPath = Assembly.GetExecutingAssembly().Location ?? throw new("Could not find assembly path.");
        string contentPath = Path.Combine(assemblyPath, "Content");
        if (!Directory.Exists(contentPath))
        {
            Directory.CreateDirectory(contentPath);
        }
        ContentPath = contentPath;
        string[] files = Directory.GetFiles(contentPath);
        foreach (string file in files)
        {
            if (Directory.Exists(file))
            {
                string[] subFiles = Directory.GetFiles(file);
                foreach (string subFile in subFiles)
                {
                    LoadFile(subFile, engine);
                }
            }
            else
            {
                LoadFile(file, engine);
            }
        }
    }

    private static void LoadFile(string file, Engine engine)
    {

    }
}
