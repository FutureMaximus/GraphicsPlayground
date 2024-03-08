using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Terrain;

/// <summary>
/// Based off of https://transvoxel.org/.
/// Lengyel, Eric. “Voxel-Based Terrain for Real-Time Virtual Simulations”. PhD diss., University of California at Davis, 2010.
/// </summary>
public class VolumeChunk<T> : IVolumeData<T>
{
    private readonly T[] _data;
    private readonly VolumeSize _size;
    public readonly Vector3i Position;

    public VolumeChunk(VolumeSize size, Vector3i position)
    {
        _size = size;
        Position = position;
        _data = new T[_size.SideLengthCubed];
    }

    public T this[int index]
    {
        get { return _data[index]; }
        set { _data[index] = value; }
    }

    #region Implementation of IVolumeData<T>

    public T this[int x, int y, int z]
    {
        get { return _data[x + y * _size.SideLength + z * _size.SideLengthSquared]; }
        set { _data[x + y * _size.SideLength + z * _size.SideLengthSquared] = value; }
    }

    public T this[Vector3i v]
    {
        get { return this[v.X, v.Y, v.Z]; }
        set { this[v.X, v.Y, v.Z] = value; }
    }

    public T this[Vector3 v]
    {
        get { return this[(int)v.X, (int)v.Y, (int)v.Z]; }
        set { this[(int)v.X, (int)v.Y, (int)v.Z] = value; }
    }

    public VolumeSize Size { get { return _size; } }

    #endregion

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        VolumeChunk<T> other = (VolumeChunk<T>)obj;
        return Position.Equals(other.Position);
    }

    public override int GetHashCode()
    {
        return Position.GetHashCode();
    }
}
