using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Models;

public interface IModelPart
{
    ///<summary>The usage hint for the model part.</summary>
    BufferUsageHint ModelUsageHint { get; }

    ///<summary>The parent of this part.</summary>
    IModelPart? Parent { get; set; }

    Matrix4 LocalTransformation { get; set; }
}
