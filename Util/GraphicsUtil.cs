using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;

namespace GraphicsPlayground.Util;

/// <summary> Reference for error code information https://registry.khronos.org/OpenGL-Refpages/gl4/. </summary>
public static class GraphicsUtil
{
    [Conditional("DEBUG")]
    public static void LoadDebugger()
    {
        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.DebugOutputSynchronous);
        GL.DebugMessageCallback(DebugCallback, IntPtr.Zero);
    }

    private static void DebugCallback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
    {
        string msg = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message, length);
        if (severity == DebugSeverity.DebugSeverityNotification)
        {
            return;
        }
        DebugLogger.Log($"<red>OpenGL Debug {severity}: {msg}");
        Debug.WriteLine($"OpenGL Debug {severity}: {msg}");
        Console.WriteLine($"OpenGL Debug {severity}: {msg}");
    }

    [Conditional("DEBUG")]
    public static void LabelObject(ObjectLabelIdentifier objLabel, int glObject, string name)
    {
        GL.ObjectLabel(objLabel, glObject, name.Length, name);
    }

    /// <summary>Checks errors when debug is enabled.</summary>
    /// <param name="callerLocationLabel">A simple text string describing the source calling location.</param>
    /// <param name="context">An optional context object.</param>
    [Conditional("DEBUG")]
    public static void CheckError(string title)
    {
        ErrorCode error;
        int i = 1;
        while ((error = GL.GetError()) != ErrorCode.NoError)
        {
            DebugLogger.Log($"<red>OpenGL Error {title} {i++}: {error}. See reference for error code information.");
            Debug.WriteLine($"OpenGL Error {title} {i++}: {error}. See reference for error code information.");
            Console.WriteLine($"OpenGL Error {title} {i++}: {error}. See reference for error code information.");
        }
    }

    /// <summary> The maximum amount of bone influences a vertex can have. </summary>
    public readonly static int MaxBoneInfluence = 4;

    public static int[] EmptyBoneIDs()
    {
        int[] boneIds = new int[MaxBoneInfluence];
        for (int i = 0; i < MaxBoneInfluence; i++)
        {
            boneIds[i] = -1;
        }

        return boneIds;
    }

    public static float[] EmptyBoneWeights()
    {
        float[] boneWeights = new float[MaxBoneInfluence];
        for (int i = 0; i < MaxBoneInfluence; i++)
        {
            boneWeights[i] = 0;
        }

        return boneWeights;
    }
}

