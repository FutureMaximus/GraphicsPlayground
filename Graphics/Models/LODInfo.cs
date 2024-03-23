namespace GraphicsPlayground.Graphics.Models;

/// <summary>Level of detail information.</summary>
public struct LODInfo(uint lod, float distance)
{
    public uint LOD = lod;
    public float Distance = distance;
}
