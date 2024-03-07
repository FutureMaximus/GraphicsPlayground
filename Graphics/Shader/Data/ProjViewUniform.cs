using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace GraphicsPlayground.Graphics.Shaders.Data;

/// <summary> 
/// The global uniform data for the projection, view, and view position.
/// 64 bytes for the projection matrix, 64 bytes for the view matrix, and 12 bytes for the view position.
/// Note that the matrices are transposed.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct ProjViewUniform
{
    public Matrix4 Projection;
    public Matrix4 View;
    public Vector3 ViewPos;

    public ProjViewUniform(Matrix4 projection, Matrix4 view, Vector3 viewPos)
    {
        projection.Transpose();
        view.Transpose();
        Projection = projection;
        View = view;
        ViewPos = viewPos;
    }
}
