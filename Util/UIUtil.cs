using ImGuiNET;
using System.Runtime.CompilerServices;

namespace GraphicsPlayground.Util;

public static class UIUtil
{
    /// <summary>
    /// Sets the drag and drop payload for ImGui drag and drop.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="title"></param>
    /// <param name="data"></param>
    /// <param name="condition"></param>
    public static unsafe void SetDragAndDropPayload<T>(string title, T data, ImGuiCond condition = 0)
        where T : unmanaged
    {
        void* ptr = Unsafe.AsPointer(ref data);
        ImGui.SetDragDropPayload(title, new IntPtr(ptr), (uint)Unsafe.SizeOf<T>(), condition);
    }

    /// <summary>
    /// If not null returns the payload data from ImGui drag and drop.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="title"></param>
    /// <returns>
    /// The payload data if it exists, otherwise null.
    /// </returns>
    public static unsafe T? AcceptPayLoad<T>(string title)
        where T : unmanaged
    {
        ImGuiPayloadPtr payLoad = ImGui.AcceptDragDropPayload(title);
        if (payLoad.NativePtr != null)
        {
            T data = Unsafe.Read<T>((void*)payLoad.Data);
            return data;
        }
        return null;
    }
}
