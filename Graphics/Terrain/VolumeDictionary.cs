using OpenTK.Mathematics;
using System.Collections.Concurrent;

namespace GraphicsPlayground.Graphics.Terrain;

/// <summary>
/// Based off of https://transvoxel.org/.
/// Lengyel, Eric. “Voxel-Based Terrain for Real-Time Virtual Simulations”. PhD diss., University of California at Davis, 2010.
/// </summary>
public class VolumeDictionary<T>(VolumeSize size) : IVolumeData<T>
{
    private readonly ConcurrentDictionary<Vector3i, VolumeChunk<T>> _data = new();
    private readonly VolumeSize _size = size;

    public VolumeSize Size
    {
        get { return _size; }
    }

    public Vector3i GetChunkIndex(int x, int y, int z)
    {
        int xI = x / Size.SideLength;
        if (x < 0) xI--;

        int yI = y / Size.SideLength;
        if (y < 0) yI--;

        int zI = z / Size.SideLength;
        if (z < 0) zI--;

        return new Vector3i(xI, yI, zI);
    }

    public T this[int x, int y, int z]
    {
        get
        {
            Vector3i chunkIndex = GetChunkIndex(x, y, z);
            if (!_data.ContainsKey(chunkIndex))
#pragma warning disable CS8603 // Possible null reference return.
                return default;
#pragma warning restore CS8603 // Possible null reference return.

            int offsetIndex = (x - chunkIndex.X * Size.SideLength) +
                              (y - chunkIndex.Y * Size.SideLength) * Size.SideLength +
                              (z - chunkIndex.Z * Size.SideLength) * Size.SideLengthSquared;

            return _data[chunkIndex][offsetIndex];
        }
        set
        {
            Vector3i chunkIndex = GetChunkIndex(x, y, z);

            int offsetIndex = (x - chunkIndex.X * Size.SideLength) +
                              (y - chunkIndex.Y * Size.SideLength) * Size.SideLength +
                              (z - chunkIndex.Z * Size.SideLength) * Size.SideLengthSquared;

            VolumeChunk<T> chunk;
            if (!_data.ContainsKey(chunkIndex))
                chunk = CreateChunk(chunkIndex);
            else
                chunk = _data[chunkIndex];

            chunk[offsetIndex] = value;
        }
    }

    public VolumeChunk<T> CreateChunk(Vector3i chunkIndex)
    {
        var chunk = new VolumeChunk<T>(Size, chunkIndex * Size.SideLength);
        _data.TryAdd(chunkIndex, chunk);
        return chunk;
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
}
