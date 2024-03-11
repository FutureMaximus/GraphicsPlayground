using GraphicsPlayground.Graphics.Terrain.Density;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Terrain.Meshing;

/// <summary>
/// Generates the mesh data for a chunk of terrain.
/// Based off of https://transvoxel.org/.
/// </summary>
public class TransvoxelMesher
{
    public readonly List<float> DensityData;
    public DensityGenerator DensityGenerator;
    public Vector3i ChunkMin;
    public int LOD;
    public int NeighborsMask;
    /// <summary> The size of the chunk. </summary>
    public int ChunkSize;
    /// <summary> The cell width of the transition cells.</summary>
    public const float TRANSITION_CELL_WIDTH_PERCENTAGE = 0.5f;

    #region Transvoxel Polygonize
    public void Polygonise()
    {
        int padding = 1;
        int lodScale = 1 << LOD;
        List<int> currentCache = new(ChunkSize * ChunkSize * 4);
        List<int> previousCache = new(ChunkSize * ChunkSize * 4);
        List<uint> vertexIndices = new(16);
        List<float> cellValues = new(8);

    }
    #endregion
}
