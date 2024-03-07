using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Animations;

public struct BoneInfo(int id, Matrix4 offset)
{
    public int ID = id;

    public Matrix4 Offset = offset;
}
