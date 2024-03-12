using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Terrain.Meshing;

public struct MeshTask
{
    public Vector3i ChunkPosition;
    public Vector3i ChunkPositionMin;
    public int LOD;
    public int NeighborsMask;
    public TransvoxelMesher Mesher;
    public bool IsComplete;

    public void Execute()
    {
        Mesher.Polygonise();
        Mesher.PolygoniseTransitions();
        IsComplete = true;
    }
}
