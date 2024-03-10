using System.Collections.Concurrent;
using GraphicsPlayground.Graphics.Render;
using GraphicsPlayground.Graphics.Terrain.Chunks;
using GraphicsPlayground.Util;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Terrain.World;

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
    /// <summary>The volume size of the world.</summary>
    public readonly int WorldSize;

    public VoxelWorld(Engine engine, int worldSize)
    {
        Chunks = new ConcurrentDictionary<Vector3, Chunk>();
        VolumeData = new VolumeDictionary<sbyte>(new VolumeSize(8));
        Extractor = new TransvoxelExtractor(VolumeData);
        Engine = engine;
        WorldSize = worldSize;
    }

    public static Dictionary<int, float> LODDistances = new()
    {
        { 1, 100f },
        //{ 2, 200f },
        /*{ 3, 1000f },
        { 4, 1500f },*/
        /*{ 5, 500f },
        { 6, 600f },
        { 7, 700f },
        { 8, 800f },
        { 9, 900f },
        { 10, 1000f },
        { 11, 1100f },
        { 12, 1200f }*/
    };

    public void TestGenerate()
    {
        Noise noise = new();
        noise.SetNoiseType(Noise.NoiseType.OpenSimplex2);
        noise.SetFractalOctaves(2);
        //noise.SetFrequency(0.01f);
        noise.SetDomainWarpAmp(0.5f);

        float scale = 1f;
        for (int x = 0; x < WorldSize; x++)
        {
            for (int y = 0; y < WorldSize; y++)
            {
                for (int z = 0; z < WorldSize; z++)
                {
                    float xCoord = x * scale;
                    float yCoord = y * scale;
                    float zCoord = z * scale;
                    VolumeData[x, y, z] = (sbyte)(noise.GetNoise(xCoord, yCoord, zCoord) * 45f);
                }
            }
        }
    }

    public void ExtractMesh(int lod)
    {
        List<TerrainMesh> meshesToLoad = [];
        int numberOfChunks = WorldSize / VolumeData.Size.SideLength;
        Console.WriteLine($"Extracting {numberOfChunks} chunks");
        SortedDictionary<int, List<TerrainMesh>> terrainMeshes = [];
        for (int x = 0; x < numberOfChunks; x++)
        {
            for (int y = 0; y < numberOfChunks; y++)
            {
                for (int z = 0; z < numberOfChunks; z++)
                {
                    Vector3i pos = new(x, y, z);
                    float distance = Vector3.DistanceSquared(pos, Engine.Camera.Position);
                    int lodLevel = GetLodLevel(distance);
                    Vector3i newPos = pos * 8 * lodLevel;
                    TerrainMesh mesh = new(newPos.X, newPos.Y, newPos.Z)
                    {
                        LOD = lodLevel
                    };
                    if (!terrainMeshes.ContainsKey(lodLevel))
                    {
                        terrainMeshes[lodLevel] = new();
                    }
                    terrainMeshes[lodLevel].Add(mesh);
                    //Extractor.GenLodRegion(ref mesh, newPos, 8, lodLevel);
                    //Console.WriteLine(lodLevel);
                    //if (mesh.Vertices.Count == 0) continue;
                    //meshesToLoad.Add(mesh);
                }
            }
        }
        foreach (KeyValuePair<int, List<TerrainMesh>> pair in terrainMeshes)
        {
            foreach (TerrainMesh mesh in pair.Value)
            {
                TerrainMesh terrainMesh = mesh;
                Extractor.GenLodRegion(ref terrainMesh, new((int)mesh.Position.X, (int)mesh.Position.Y, (int)mesh.Position.Z), 8, pair.Key);
                if (mesh.Vertices.Count == 0) continue;
                meshesToLoad.Add(mesh);
                Console.WriteLine($"Mesh {mesh.Position.X},{mesh.Position.Y},{mesh.Position.Z} with LOD {pair.Key} generated. Vertices: {mesh.Vertices.Count}");
            }
        }
        foreach (TerrainMesh mesh in meshesToLoad)
        {
            mesh.Load();
            //Console.WriteLine($"Mesh {mesh.Position.X},{mesh.Position.Y},{mesh.Position.Z} loaded. Vertices: {mesh.Vertices.Count}");
            Engine.TerrainMeshes.Add(mesh);
        }


        static int GetLodLevel(float distance)
        {
            foreach (KeyValuePair<int, float> pair in LODDistances)
            {
                if (distance < pair.Value)
                {
                    return pair.Key;
                }
            }
            KeyValuePair<int, float> last = LODDistances.Last();
            return last.Key;
        }
    }
}
