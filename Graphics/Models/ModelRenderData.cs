using System.Drawing;

namespace GraphicsPlayground.Graphics.Models;

public class ModelRenderData
{
    /// <summary> Whether the model has shadow calculations. </summary>
    public bool ShadowEnabled = true;

    /// <summary> Whether the model is visible this will stop it from rendering if false. </summary>
    public bool Visible = true;

    /// <summary> The color of the model. </summary>
    public Color Color = Color.White;
}
