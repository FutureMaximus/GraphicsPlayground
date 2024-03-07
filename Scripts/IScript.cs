using GraphicsPlayground.Graphics.Render;

namespace GraphicsPlayground.Scripts;

/// <summary>Scripts that can be loaded dynamically into the engine.</summary>
public interface IScript
{
    /// <summary> Passes the engine to the script and loads it. </summary>
    /// <param name="engine"></param>
    public void OnLoad(Engine engine);

    /// <summary> Custom run logic for the script this is executed while the engine is running. </summary>
    public void Run();

    /// <summary> When the engine updates this will fire every frame if ShouldUpdate is enabled. </summary>
    public void Update();

    /// <summary> Fires when the script is unloaded. </summary>
    public void OnUnload();

    /// <summary> Whether the script should be updated defaults to true. </summary>
    public bool ShouldUpdate { get { return true; } }

    /// <summary> The description of the script defaults to null if not specified. </summary>
    public string? Description { get { return null; } }

    /// <summary> Whether the script is enabled or not defaults to true if not specified. </summary>
    public bool? IsEnabled { get { return true; } }
}
