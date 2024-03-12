namespace GraphicsPlayground.Graphics.Terrain.Meshing;

public struct TerrainMeshRenderContainer
{
    public TerrainMeshRenderData MainData;
    public TerrainMeshRenderData LeftTransitionData;
    public TerrainMeshRenderData RightTransitionData;
    public TerrainMeshRenderData UpTransitionData;
    public TerrainMeshRenderData DownTransitionData;
    public TerrainMeshRenderData ForwardTransitionData;
    public TerrainMeshRenderData BackTransitionData;

    public TerrainMeshRenderContainer()
    {
        MainData = new();
        LeftTransitionData = new();
        RightTransitionData = new();
        UpTransitionData = new();
        DownTransitionData = new();
        ForwardTransitionData = new();
        BackTransitionData = new();
    }
}
