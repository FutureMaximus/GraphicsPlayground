namespace GraphicsPlayground.Graphics.Terrain.Meshing;

public struct TerrainMeshRenderContainer(TerrainMeshRenderData mData, TerrainMeshRenderData lTData, TerrainMeshRenderData rTData, TerrainMeshRenderData uTData, TerrainMeshRenderData dTData, TerrainMeshRenderData fTData, TerrainMeshRenderData bTData) : IDisposable
{
    public TerrainMeshRenderData MainData = mData;
    public TerrainMeshRenderData LeftTransitionData = lTData;
    public TerrainMeshRenderData RightTransitionData = rTData;
    public TerrainMeshRenderData UpTransitionData = uTData;
    public TerrainMeshRenderData DownTransitionData = dTData;
    public TerrainMeshRenderData ForwardTransitionData = fTData;
    public TerrainMeshRenderData BackTransitionData = bTData;

    public readonly void Dispose()
    {
        MainData.Dispose();
        LeftTransitionData.Dispose();
        RightTransitionData.Dispose();
        UpTransitionData.Dispose();
        DownTransitionData.Dispose();
        ForwardTransitionData.Dispose();
        BackTransitionData.Dispose();
        GC.SuppressFinalize(this);
    }
}
