using GraphicsPlayground.Util;

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

        Console.WriteLine("Hello, World!");
    }
}
