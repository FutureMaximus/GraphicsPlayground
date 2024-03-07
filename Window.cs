using GraphicsPlayground.Graphics.Render;
using GraphicsPlayground.Util;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Envision;

public class Window : GameWindow
{
    public int ScreenHeight;
    public int ScreenWidth;
    public Engine Engine;
    public ImGuiRender? ImGuiRenderer;
    public CameraControl? CameraState;

    public Window(int width, int height, ContextFlags flags) : base(GameWindowSettings.Default, new NativeWindowSettings()
    {
        ClientSize = (width, height),
        Title = "Envision",
        WindowBorder = WindowBorder.Resizable,
        WindowState = WindowState.Normal,
        StartVisible = true,
        StartFocused = true,
        Vsync = VSyncMode.On,
        API = ContextAPI.OpenGL,
        APIVersion = new Version(4, 6),
        Profile = ContextProfile.Core,
        Flags = flags
    }
    )
    {
        ScreenWidth = width;
        ScreenHeight = height;
        CursorState = CursorState.Grabbed;
        Engine = new(this);
        CenterWindow(); // Center the window on the screen
    }

    protected override void OnLoad()
    {
        CameraState = new(Engine.Camera);
        Engine.Window = this;

        Engine.Load();
        ImGuiRenderer = new(ClientSize.X, ClientSize.Y);

        base.OnLoad();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        float time = (float)args.Time;
        Engine.FPS = (int)(1 / time);
        ImGuiRenderer?.Update(this, time);
        Engine.Render();
        //ImGuiNET.ImGui.ShowDebugLogWindow();
        // Stop saving the windows
        //ImGuiNET.ImGui.ShowDemoWindow();
        GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
        ImGuiRenderer?.Render();
        ImGuiRender.CheckGLError("End of frame");
        SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        float deltaTime = (float)args.Time;

        KeyboardState keyboardState = KeyboardState;
        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
        }
        if (CameraState is not null)
        {
            CameraState.UpdateKeyboardState(keyboardState, deltaTime);
            if (!CameraState.IsMoving)
            {
                CursorState = CursorState.Normal;
            }
            else
            {
                CursorState = CursorState.Grabbed;
                CameraState.UpdateMouseState(MouseState);
            }
        }
        Engine.Update(deltaTime);
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
        Engine.EngineSettings.WindowSize = new Vector2i(ClientSize.X, ClientSize.Y);
        Engine.EngineSettings.AspectRatio = ClientSize.X / (float)ClientSize.Y;
        ImGuiRenderer?.WindowResized(ClientSize.X, ClientSize.Y);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        ImGuiRenderer?.MouseScroll(e.Offset);
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);
        ImGuiRenderer?.PressChar((char)e.Unicode);
    }

    private void ShutDown()
    {
        Config.Save();
        Engine.ShutDown();
        ImGuiRenderer?.DestroyDeviceObjects();
        Dispose();
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        ShutDown();
    }
}
