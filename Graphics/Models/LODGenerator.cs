using GraphicsPlayground.Util;
using System.Numerics;

namespace GraphicsPlayground.Graphics.Models;

///<summary>Represents a triangle in the level of detail generator.</summary>
public struct LODGenTriangle
{
    public readonly int[] Vertices;
    public readonly double[] Error;
    public int Deleted;
    public int Dirty;
    public int Attribute;
    public Vector3 Normal;
    public Vector3[] UVs;
    public int Material;

    public LODGenTriangle()
    {
        Vertices = new int[3];
        Error = new double[4];
        Deleted = 0;
        Dirty = 0;
        Attribute = 0;
        Normal = Vector3.Zero;
        UVs = new Vector3[3];
        Material = 0;
    }
}

///<summary>Represents a vertex in the level of detail generator.</summary>
public struct LODGenVertex(Vector3 point, int tstart, int tcount, SymmetricMatrix q, int borderVertex)
{
    public Vector3 Point = point;
    public int TStart = tstart;
    public int TCount = tcount;
    public SymmetricMatrix Q = q;
    public int BorderVertex = borderVertex;
}

///<summary>Represents a reference in a level of detail generator.</summary>
public struct LODGenRef(int tID, int tVertex)
{
    public int TID = tID;
    public int TVertex = tVertex;
}

///<summary>Generates several levels of detail for a model.</summary>
public static class LODGenerator
{
    public static IMesh[] GenerateLODs(in IMesh refMesh, int levels, float maximumDistance, double aggressiveess = 7)
    {
        int meshVertCount = refMesh.Vertices.Count;
        if (meshVertCount == 0)
        {
            throw new ArgumentException("Vertices are empty.");
        }
        IMesh[] lods = new IMesh[levels];
        for (int i = 0; i < levels; i++)
        {
            lods[i] = GenerateLOD(refMesh, i, maximumDistance, aggressiveess);
        }
        return lods;
    }

    public static IMesh GenerateLOD(in IMesh refMesh, int level, float maximumDistance, int targetCount, double aggressiveess)
    {
        List<LODGenTriangle> triangles = new();
        List<LODGenVertex> vertices = new();
        List<LODGenRef> refs = new();

    }

    public class InternalMethods
    {

    }
}
