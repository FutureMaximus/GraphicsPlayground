using System.Collections.Concurrent;
using GraphicsPlayground.Graphics.Render;
using GraphicsPlayground.Util;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Terrain;

/// <summary>The voxel world that contains all the data for the terrain.</summary>
public class VoxelWorld
{
    /// <summary>Reference to the engine.</summary>
    public Engine Engine;
    /// <summary>The chunks that make up the world.</summary>
    public readonly ConcurrentDictionary<Vector3, Chunk> Chunks;
    /// <summary>The volume data for the world this is where you modify the terrain.</summary>
    public readonly IVolumeData<sbyte> VolumeData;
    /// <summary>The mesh extractor for the terrain.</summary>
    public TransvoxelExtractor Extractor;
    /// <summary>The world location of the voxel world.</summary>
    public readonly Matrix4 WorldLocation = Matrix4.Identity;

    public VoxelWorld(Engine engine)
    {
        Chunks = new ConcurrentDictionary<Vector3, Chunk>();
        VolumeData = new VolumeDictionary<sbyte>(new VolumeSize(8));
        Extractor = new TransvoxelExtractor(VolumeData);
        Engine = engine;
    }

    public void TestGenerate(int volumeSize)
    {
        Noise noise = new();
        noise.SetNoiseType(Noise.NoiseType.OpenSimplex2);
        noise.SetFractalOctaves(4);

        for (int x = 0; x < volumeSize; x++)
        {
            for (int y = 0; y < volumeSize; y++)
            {
                for (int z = 0; z < volumeSize; z++)
                {
                    float noiseVal = (noise.GetNoise(x, y, z) + 1) * 127.5f;
                    VolumeData[x, y, z] = (sbyte)noiseVal;
                    // Sphere SDF
                    //float radius = 20;
                    //float distance = (float)Math.Sqrt(x * x + y * y + z * z) - 20;
                    //VolumeData[x, y, z] = (sbyte)distance;
                }
            }
        }
    }

    public void ExtractMesh(int size, int lod)
    {
        List<TerrainMesh> meshesToLoad = [];
        int numberOfChunks = size / VolumeData.Size.SideLength;
        // TODO: Use single thread for this.
        for (int x = 0; x < numberOfChunks; x++)
        {
            for (int y = 0; y < numberOfChunks; y++)
            {
                for (int z = 0; z < numberOfChunks; z++)
                {
                    Vector3i pos = new(y, x, z);
                    Vector3i newPos = pos * 8 * lod;
                    TerrainMesh mesh = new(newPos.X, newPos.Y, newPos.Z);
                    Extractor.GenLodRegion(ref mesh, newPos, 8, lod);
                    if (mesh.Vertices.Count == 0) continue;
                    meshesToLoad.Add(mesh);
                }
            }
        }
        foreach (TerrainMesh mesh in meshesToLoad)
        {
            mesh.Load();
            mesh.Indices.Reverse();
            Console.WriteLine($"Mesh {mesh.Position.X},{mesh.Position.Y},{mesh.Position.Z} loaded. Vertices: {mesh.Vertices.Count}");
            Engine.TerrainMeshes.Add(mesh);
        }

    }
}
