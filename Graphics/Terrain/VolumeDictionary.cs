using OpenTK.Mathematics;
using System.Collections.Concurrent;

namespace GraphicsPlayground.Graphics.Terrain;

/// <summary>
/// Based off of https://transvoxel.org/.
/// Lengyel, Eric. “Voxel-Based Terrain for Real-Time Virtual Simulations”. PhD diss., University of California at Davis, 2010.
/// </summary>
public class VolumeDictionary<T>(VolumeSize size, VolumeSize chunkSize) : IVolumeData<T>
{
    private readonly ConcurrentDictionary<Vector3i, VolumeChunk<T>> _data = new();
    private readonly VolumeSize _size = size;
    private readonly VolumeSize _chunkSize = chunkSize;

    public VolumeSize Size
    {
        get { return _size; }
    }

    public VolumeSize ChunkSize
    {
        get { return _chunkSize; }
    }

    private Vector3i GetChunkIndex(int x, int y, int z)
    {
        int xI = (int)(x / ChunkSize.SideLength);
        if (x < 0) xI--;

        int yI = (int)(y / ChunkSize.SideLength);
        if (y < 0) yI--;

        int zI = (int)(z / ChunkSize.SideLength);
        if (z < 0) zI--;

        return new Vector3i(xI, yI, zI);
    }

    public T this[int x, int y, int z]
    {
        get
        {
#pragma warning disable CS8603
            Vector3i chunkIndex = GetChunkIndex(x, y, z);
            if (!_data.TryGetValue(chunkIndex, out VolumeChunk<T>? value))
                return default;
#pragma warning restore CS8603

            long offsetIndex = x - chunkIndex.X * ChunkSize.SideLength +
                              (y - chunkIndex.Y * ChunkSize.SideLength) * ChunkSize.SideLength +
                              (z - chunkIndex.Z * ChunkSize.SideLength) * ChunkSize.SideLengthSquared;

            return value[(int)offsetIndex];
        }
        set
        {
            Vector3i chunkIndex = GetChunkIndex(x, y, z);

            long offsetIndex = (x - chunkIndex.X * ChunkSize.SideLength) +
                              (y - chunkIndex.Y * ChunkSize.SideLength) * ChunkSize.SideLength +
                              (z - chunkIndex.Z * ChunkSize.SideLength) * ChunkSize.SideLengthSquared;

            VolumeChunk<T> chunk;
            if (!_data.ContainsKey(chunkIndex))
                chunk = CreateChunk(chunkIndex);
            else
                chunk = _data[chunkIndex];

            chunk[(int)offsetIndex] = value;
        }
    }

    public VolumeChunk<T> GetChunk(Vector3 position)
    {
        Vector3i chunkIndex = GetChunkIndex((int)position.X, (int)position.Y, (int)position.Z);
        if (!_data.TryGetValue(chunkIndex, out VolumeChunk<T>? value))
            return CreateChunk(chunkIndex);

        return value;
    }

    private VolumeChunk<T> CreateChunk(Vector3i chunkIndex)
    {
        VolumeChunk<T> chunk = new(Size, chunkIndex * (int)ChunkSize.SideLength);
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
