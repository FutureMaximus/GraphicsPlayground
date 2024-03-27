using GraphicsPlayground.Util;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Models;

///<summary>Represents a triangle in the level of detail generator.</summary>
public struct LODGenTriangle
{
    public readonly int[] Vertices;
    public double[] Error;
    public bool Deleted;
    public bool Dirty;
    public int Attribute;
    public Vector3 Normal;
    public Vector3[] UVs;
    public int Material;

    public LODGenTriangle()
    {
        Vertices = new int[3];
        Error = new double[4];
        Deleted = false;
        Dirty = false;
        Attribute = 0;
        Normal = Vector3.Zero;
        UVs = new Vector3[3];
        Material = 0;
    }
}

///<summary>Represents a vertex in the level of detail generator.</summary>
public struct LODGenVertex(Vector3 point)
{
    public Vector3 Point = point;
    public int TStart = 0;
    public int TCount = 0;
    public SymmetricMatrix Q = new();
    public bool Border = false;
    public bool Seam = false;
    public bool Foldover = false;
}

///<summary>Represents a reference in a level of detail generator.</summary>
public struct LODGenRef(int tID, int tVertex)
{
    public int TID = tID;
    public int TVertex = tVertex;
}

///<summary>
///Generates several levels of detail for a model.
///Decimation algorithm based on https://github.com/sp4cerat/Fast-Quadric-Mesh-Simplification
///</summary>
public static class LODGenerator
{
    /// <summary>Generates levels of detail for a mesh.</summary>
    /// <param name="refMesh">The referenced mesh to generate the LODs for.</param>
    /// <param name="levels">The amount of level of details.</param>
    /// <param name="maxRenderDistance">The maximum render distance until this mesh can no longer be rendered.</param>
    /// <param name="minRenderDistance">The minimum render distance for LOD 1</param>
    /// <param name="lodVertFactor">The factor for the amount of decreased vertices in each LOD must be between 0.0-1.0.</param>
    /// <param name="aggressiveness">How aggressive the algorithm should edge collapse higher values equals more quality at the cost of performance</param>
    /// <exception cref="ArgumentException"></exception>
    public static IMesh[] GenerateLODs(in IMesh refMesh, uint levels, float maxRenderDistance, float minRenderDistance = 100, double lodVertFactor = 0.7, double aggressiveness = 7)
    {
        int meshVertCount = refMesh.Vertices.Count;
        if (meshVertCount == 0)
        {
            throw new ArgumentException("Vertices are empty.");
        }
        else if (maxRenderDistance <= 0)
        {
            throw new ArgumentException("Maximum render distance must be greater than 0.");
        }
        else if (minRenderDistance <= 0)
        {
            throw new ArgumentException("Minimum render distance must be greater than 0.");
        }
        else if (minRenderDistance >= maxRenderDistance)
        {
            throw new ArgumentException("Minimum render distance must be less than maximum render distance.");
        }
        else if (lodVertFactor <= 0 || lodVertFactor >= 1)
        {
            throw new ArgumentException("LOD vertex factor must be between 0.0 and 1.0.");
        }
        IMesh[] lodMeshes = new IMesh[levels];
        /*for (uint i = 1; i <= levels; i++)
        {
            double distance = InternalMethods.GetLODDistance(minRenderDistance, maxRenderDistance, i, levels);
            uint targetCount = InternalMethods.GetLODTargetCount(meshVertCount, lodVertFactor, i);
            lodMeshes[i - 1] = GenerateLOD(refMesh, targetCount, aggressiveness);
            lodMeshes[i - 1].LOD = new LODInfo(i, (float)distance);
        }*/
        return lodMeshes;
    }

    ///<summary>Generates a level of detail mesh.</summary>
    /*public static IMesh GenerateLOD(in IMesh refMesh, uint targetCount, double aggressiveness)
    {
        List<LODGenTriangle> triangles = new(refMesh.Indices.Count / 3);
        List<LODGenVertex> vertices = new(refMesh.Vertices.Count);
        List<LODGenRef> refs = [];
        for (int i = 0; i < refMesh.Vertices.Count; i++)
        {
            vertices.Add(new LODGenVertex()
            {
                Point = refMesh.Vertices[i]
            });
        }
        for (int i = 0; i < refMesh.Indices.Count; i += 3)
        {
            LODGenTriangle triangle = new();
            triangle.Vertices[0] = (int)refMesh.Indices[i];
            triangle.Vertices[1] = (int)refMesh.Indices[i + 1];
            triangle.Vertices[2] = (int)refMesh.Indices[i + 2];
            triangles.Add(triangle);
        }
        int deletedTriangles = 0;
        List<bool> deleted0 = new(20);
        List<bool> deleted1 = new(20);
        for (int i = 0; i < 100; i++)
        {
            if (triangles.Count - deletedTriangles <= targetCount)
            {
                break;
            }
            if (i % 5 == 0)
            {
                InternalMethods.UpdateMesh(i, triangles, vertices, refs);
            }
            for (int t = 0; t < triangles.Count; t++)
            {
                LODGenTriangle triangle = triangles[i];
                triangle.Dirty = false;
                triangles[t] = triangle;
            }
            double threshold = 0.000000001 * Math.Pow(i + 3, aggressiveness);

        }
    }*/

    #region Internal Methods
    public class InternalMethods
    {
        ///<summary>Gets the distance for a level of detail.</summary>
        public static double GetLODDistance(double start, double end, uint lod, uint numberOfLods)
        {
            double logRatio = Math.Log(end / start);
            double baseLog = logRatio / numberOfLods;
            double distance = Math.Exp(baseLog * lod) * start;
            return Math.Min(distance, end);
        }

        ///<summary>Gets the decreased amount of triangles for a LOD mesh.</summary>
        public static uint GetLODTargetCount(int vertexCount, double factor, uint level)
        {
            return (uint)(vertexCount * Math.Pow(factor, level) / 3);
        }

        ///<summary>Updates the LOD meshes triangles.</summary>
        public static void UpdateMesh(int iteration, List<LODGenTriangle> trianglesToUpdate, List<LODGenVertex> verticesToUpdate, List<LODGenRef> refs)
        {
            if (iteration > 0)
            {
                int dst = 0;
                for (int i = 0; i < trianglesToUpdate.Count; i++)
                {
                    LODGenTriangle triangle = trianglesToUpdate[i];
                    if (!triangle.Deleted)
                    {
                        if (dst != i)
                        {
                            trianglesToUpdate[dst] = triangle;
                        }
                        dst++;
                    }
                }
                trianglesToUpdate.Capacity = dst;
            }
        }

        ///<summary>Updates the references for the LOD generator.</summary>
        public static void UpdateRefs(List<LODGenTriangle> triangles, List<LODGenVertex> vertices, List<LODGenRef> refs)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                LODGenVertex vertex = vertices[i];
                vertex.TStart = 0;
                vertex.TCount = 0;
                vertices[i] = vertex;
            }
        }
    }
    #endregion
}
