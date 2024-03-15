using System.Numerics;

namespace GraphicsPlayground.Util;

public static class Config
{
    public static InternalSettings Settings
    {
        get => _settings;
        set
        {
            _settings = value;
        }
    }
    private static InternalSettings _settings;

    public struct InternalSettings
    {
        public Vector2 Resolution;
        public Vector2 MousePosition;
        public Vector2 MouseSensitivity;
        public string Font;
        public float FontSizePixels;
        public string? FontPath;
        public string ShaderPath;
        public string AssemblyName;
        public string ScriptPath;
        public bool DebugMode;
    }
}
