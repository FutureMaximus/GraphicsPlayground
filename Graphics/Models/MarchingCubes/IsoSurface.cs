using GraphicsPlayground.Graphics.Models.Generic;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Models.MarchingCubes;

/// <summary> 
/// General purpose isosurface class for constructing meshes from marching cubes. 
/// Try not to go below 1 for the cell size, big performance hit.
/// </summary>
public class IsoSurface
{
    public GenericMesh? Mesh;
    /// <summary> Function to calculate the value of the isosurface. </summary>
    public Func<Vector3, float> Function = (v) => 0.0f;
    public readonly float Epsilon = 0.0003f;
    public float CellSize;
    public Vector3 Start, End, Diff;
    public int XCellsCount, YCellsCount, ZCellsCount;

    private uint[] _indiceStorage = new uint[15];
    private Vector3[] _vertexStorage = new Vector3[15];
    private Vector3[] _normalStorage = new Vector3[15];

    /// <summary>
    /// Calculates the gradient of the isosurface at the given position.
    /// Utilized for normal calculation.
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="normal"></param>
    public void CalcGradient(in Vector3 pos, ref Vector3 normal)
    {
        float value = Function(pos);
        normal.X = Function(new Vector3(pos.X + Epsilon, pos.Y, pos.Z)) - value;
        normal.Y = Function(new Vector3(pos.X, pos.Y + Epsilon, pos.Z)) - value;
        normal.Z = Function(new Vector3(pos.X, pos.Y, pos.Z + Epsilon)) - value;
        normal.Normalize();
    }

    public void IsoApproximate(ref GenericMesh mesh, float boxSize, float cellSize, Func<Vector3, float> func)
    {
        End = new Vector3(boxSize, boxSize, boxSize);
        IsoApproximate(ref mesh, -End, End, cellSize, func);
    }

    public void IsoApproximate(ref GenericMesh mesh, float xSize, float ySize, float zSize, float cellSize, Func<Vector3, float> func)
    {
        End = new Vector3(xSize, ySize, zSize);
        IsoApproximate(ref mesh, -End, End, cellSize, func);
    }

    private void IsoApproximate(ref GenericMesh mesh, Vector3 start, Vector3 end, float cellSize, Func<Vector3, float> func)
    {
        Mesh = mesh;
        Function = func;
        CellSize = cellSize;
        Start = start;
        End = end;
        Diff = End - Start;
        XCellsCount = (int)(Diff.X / CellSize);
        YCellsCount = (int)(Diff.Y / CellSize);
        ZCellsCount = (int)(Diff.Z / CellSize);

        float[] topGrid = new float[XCellsCount * YCellsCount];
        float[] bottomGrid = new float[XCellsCount * YCellsCount];

        FillGrid(ref topGrid, 0);
        for (uint z = 1; z < ZCellsCount; z++)
        {
            FillGrid(ref bottomGrid, (int)z);
            PolygonizeGrids(in topGrid, in bottomGrid, (int)z);
            Swap(ref topGrid, ref bottomGrid);
        }
    }

    private void PushPolygons(in GridCell g)
    {
        if (Mesh is null || Mesh.Vertices is null) return;
        int newVertexCount = Mesh.Vertices.Count;
        int indexShift = newVertexCount;
        int newIndicesCount = MarchingCube.Polygonise(in g, ref _indiceStorage, ref newVertexCount, ref _vertexStorage, ref _normalStorage);
        if (newIndicesCount == 0) return;

        for (int i = 0; i < newVertexCount; i++)
        {
            Mesh.Vertices.Add(_vertexStorage[i]);
        }
        for (int i = 0; i < newVertexCount; i++)
        {
            Vector3 normal = Vector3.Zero;
            CalcGradient(Mesh.Vertices[i], ref normal);
            Mesh.Normals?.Add(normal);
        }
        for (int i = 0; i < newIndicesCount; i++)
        {
            Mesh.Indices?.Add((uint)(_indiceStorage[i] + indexShift));
        }
    }

    private void FillGrid(ref float[] grid, int z)
    {
        for (uint x = 0; x < XCellsCount; x++)
        {
            for (uint y = 0; y < YCellsCount; y++)
            {
                Vector3 pos = new(Start.X + CellSize * x, Start.Y + CellSize * y, Start.Z + CellSize * (z - 1));
                grid[x * YCellsCount + y] = Function(pos);
            }
        }
    }

    private void PolygonizeGrids(in float[] topVals, in float[] bottomVals, int z)
    {
        for (uint x = 0; x < XCellsCount - 1; x++)
        {
            for (uint y = 0; y < YCellsCount - 1; y++)
            {
                GridCell g = new((int)x, (int)y, z);

                g.Vertices[0] = new Vector3(Start.X + CellSize * x, Start.Y + CellSize * y, Start.Z + CellSize * z);
                g.Vertices[1] = new Vector3(Start.X + CellSize * (x + 1), Start.Y + CellSize * y, Start.Z + CellSize * z);
                g.Vertices[2] = new Vector3(Start.X + CellSize * (x + 1), Start.Y + CellSize * (y + 1), Start.Z + CellSize * z);
                g.Vertices[3] = new Vector3(Start.X + CellSize * x, Start.Y + CellSize * (y + 1), Start.Z + CellSize * z);

                g.Values[0] = topVals[x * YCellsCount + y];
                g.Values[1] = topVals[(x + 1) * YCellsCount + y];
                g.Values[2] = topVals[(x + 1) * YCellsCount + (y + 1)];
                g.Values[3] = topVals[x * YCellsCount + (y + 1)];

                g.Vertices[4] = new Vector3(Start.X + CellSize * x, Start.Y + CellSize * y, Start.Z + CellSize * (z + 1));
                g.Vertices[5] = new Vector3(Start.X + CellSize * (x + 1), Start.Y + CellSize * y, Start.Z + CellSize * (z + 1));
                g.Vertices[6] = new Vector3(Start.X + CellSize * (x + 1), Start.Y + CellSize * (y + 1), Start.Z + CellSize * (z + 1));
                g.Vertices[7] = new Vector3(Start.X + CellSize * x, Start.Y + CellSize * (y + 1), Start.Z + CellSize * (z + 1));

                g.Values[4] = bottomVals[x * YCellsCount + y];
                g.Values[5] = bottomVals[(x + 1) * YCellsCount + y];
                g.Values[6] = bottomVals[(x + 1) * YCellsCount + (y + 1)];
                g.Values[7] = bottomVals[x * YCellsCount + (y + 1)];

                bool valid = true;
                for (uint vertexIndex = 0; vertexIndex < 8 && valid; vertexIndex++)
                {
                    if (g.Values[vertexIndex] == float.MaxValue)
                    {
                        valid = false;
                    }
                }
                if (valid)
                {
                    PushPolygons(in g);
                }
            }
        }
    }

    private static void Swap<T>(ref T a, ref T b) => (a, b) = (b, a);
}
