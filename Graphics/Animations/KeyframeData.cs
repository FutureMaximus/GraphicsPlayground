using System.Runtime.InteropServices;

namespace GraphicsPlayground.Graphics.Animations;

[StructLayout(LayoutKind.Sequential)]
public struct KeyframeData<T>(double time, T value)
{
    public readonly double Time => time;
    public readonly T Value => value;

    public readonly override string ToString() => $"Keyframe - Time: {Time}, Value: {Value}";
}
