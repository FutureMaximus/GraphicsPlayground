using GraphicsPlayground.Graphics.Models.MarchingCubes.Terrain;
using GraphicsPlayground.Util;
using OpenTK.Windowing.Common;
using System.Runtime.InteropServices;

namespace GraphicsPlayground;

public class Program
{
    static void Main(string[] args)
    {
        Config.Settings = new()
        {
            DebugMode = true,
            Resolution = new(1920, 1080),
            MousePosition = new(0, 0),
            MouseSensitivity = new(0.1f, 0.1f),
            Font = "Arial",
            FontSizePixels = 16,
            FontPath = null,
            ShaderPath = "C:\\Users\\ruben\\source\\repos\\GraphicsPlayground\\Graphics\\Shader\\InternalShaders",
            ScriptPath = "C:\\Users\\ruben\\source\\repos\\GraphicsPlayground\\Scripts\\InternalScripts"
        };

        ContextFlags contextFlags;
#if DEBUG
        contextFlags = ContextFlags.Debug | ContextFlags.ForwardCompatible;
#else
        contextFlags = ContextFlags.ForwardCompatible;
#endif

        Window window = new((int)Config.Settings.Resolution.X, (int)Config.Settings.Resolution.Y, contextFlags)
        {
            UpdateFrequency = 60,
        };
        window.Run();
    }
}
