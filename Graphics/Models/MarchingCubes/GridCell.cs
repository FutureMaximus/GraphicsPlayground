using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Models.MarchingCubes;

/// <summary> Grid cell for marching cubes. </summary>
public struct GridCell(int x, int y, int z)
{
    public int X = x, Y = y, Z = z;
    public Vector3[] Vertices = new Vector3[8];
    public float[] Values = new float[8];

    public readonly override bool Equals(object? obj)
    {
        if (obj is GridCell cell)
        {
            return cell.X == X && cell.Y == Y && cell.Z == Z;
        }
        return false;
    }

    public readonly override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    public static bool operator ==(GridCell left, GridCell right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(GridCell left, GridCell right)
    {
        return !(left == right);
    }
}
