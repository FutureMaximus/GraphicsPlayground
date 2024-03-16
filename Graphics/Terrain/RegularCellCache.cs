using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
namespace GraphicsPlayground.Graphics.Terrain;

public class RegularCellCache
{
    private readonly ReuseCell[][] _cache;
    private readonly int _chunkSize;

    public RegularCellCache(int chunksize)
    {
        _chunkSize = chunksize;
        _cache = new ReuseCell[2][];

        _cache[0] = new ReuseCell[_chunkSize * _chunkSize];
        _cache[1] = new ReuseCell[_chunkSize * _chunkSize];

        for (int i = 0; i < _chunkSize * _chunkSize; i++)
        {
            _cache[0][i] = new ReuseCell(4);
            _cache[1][i] = new ReuseCell(4);
        }
    }

    public ReuseCell GetReusedIndex(Vector3i pos, byte rDir)
    {
        int rx = rDir & 0x01;
        int rz = (rDir >> 1) & 0x01;
        int ry = (rDir >> 2) & 0x01;

        int dx = pos.X - rx;
        int dy = pos.Y - ry;
        int dz = pos.Z - rz;

        return _cache[dx & 1][dy * _chunkSize + dz];
    }


    public ReuseCell this[int x, int y, int z]
    {
        set
        {
            _cache[x & 1][y * _chunkSize + z] = value;
        }
    }

    public ReuseCell this[Vector3i v]
    {
        set { this[v.X, v.Y, v.Z] = value; }
    }


    public void SetReusableIndex(Vector3i pos, byte reuseIndex, ushort p)
    {
        _cache[pos.X & 1][pos.Y * _chunkSize + pos.Z].Verts[reuseIndex] = p;
    }
}
