using System.Collections.Concurrent;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Terrain;

/// <summary>The voxel world that contains all the data for the terrain.</summary>
public class VoxelWorld
{
    /// <summary>The chunks that make up the world.</summary>
    public readonly ConcurrentDictionary<Vector3, Chunk> Chunks;
    /// <summary>The volume data for the world this is where you modify the terrain.</summary>
    public readonly IVolumeData<sbyte> VolumeData;
    /// <summary>The mesh extractor for the terrain.</summary>
    public TransvoxelExtractor Extractor;
    /// <summary>The world location of the voxel world.</summary>
    public readonly Matrix4 WorldLocation = Matrix4.Identity;

    public VoxelWorld()
    {
        Chunks = new ConcurrentDictionary<Vector3, Chunk>();
        VolumeData = new VolumeDictionary<sbyte>(new VolumeSize(8));
        Extractor = new TransvoxelExtractor(VolumeData);
    }
}
