using GraphicsPlayground.Graphics.Render;
using GraphicsPlayground.Graphics.Terrain.Chunks;
using GraphicsPlayground.Graphics.Terrain.Density;
using GraphicsPlayground.Graphics.Terrain.World;
using GraphicsPlayground.Util;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Terrain.Meshing;

/// <summary>Responsible for updating the meshes of the terrain.</summary>
public class MeshUpdateExecutor
{
    public Engine Engine;
    public WorldSettings WorldSettings;
    public WorldState WorldState;
    public GeneratorSettings GeneratorSettings;
    public List<MeshTask> MeshTasks = [];
    public DensityGenerator DensityGenerator;

    public MeshUpdateExecutor(Engine engine, WorldSettings worldSettings, ref WorldState worldState, GeneratorSettings generatorSettings)
    {
        Engine = engine;
        WorldSettings = worldSettings;
        WorldState = worldState;
        GeneratorSettings = generatorSettings;
        DensityGenerator = new(GeneratorSettings);
    }

    public void Start()
    {
        for (int i = 0; i < WorldState.ChunkData.ChunkUpdates.Count; i++)
        {
            StartChunkTask(WorldState.ChunkData.ChunkUpdates[i]);
        }
    }

    public void StartChunkTask(in ChunkUpdate chunkUpdate)
    {
        if (chunkUpdate.UpdateType == ChunkUpdateType.Remove && WorldState.ChunkData.ChunkMap.ContainsKey(chunkUpdate.Position))
        {
            WorldState.ChunkData.ChunksToRemove.Add(chunkUpdate.Position);
        }
        else if (chunkUpdate.UpdateType == ChunkUpdateType.Create)
        {
            MeshTask meshTask = new()
            {
                ChunkPosition = chunkUpdate.Position,
                ChunkPositionMin = CoordinateUtilities.GetChunkMin(chunkUpdate.Position, WorldSettings.ChunkSize, chunkUpdate.LOD),
                LOD = chunkUpdate.LOD,
                NeighborsMask = chunkUpdate.NeighborsMask,
                IsComplete = false
            };
            MeshTasks.Add(meshTask);
        }
    }

    public void Execute()
    {
        for (int i = MeshTasks.Count - 1; i >= 0; i--)
        {
            if (WorldState.ChunkData.ChunkMap.ContainsKey(MeshTasks[i].ChunkPosition))
            {
                MeshTasks.RemoveAt(i);
                continue;
            }
            MeshTask task = MeshTasks[i];
            if (task.IsComplete)
            {
                MeshTasks.RemoveAt(i);
                Vector3i chunkPos = task.ChunkPosition;
                if (WorldState.ChunkData.ChunkMap.ContainsKey(chunkPos))
                {
                    continue;
                }
            }
            TransvoxelMesher mesher = new(WorldState.VolumeDensityData[task.ChunkPosition], DensityGenerator)
            {
                ChunkMin = task.ChunkPositionMin,
                ChunkSize = WorldSettings.ChunkSize,
                LOD = task.LOD,
                NeighborsMask = task.NeighborsMask,
                MeshDataContainer = new()
            };
            //Console.WriteLine($"Starting mesh task for chunk with lod {task.LOD}");
            task.Mesher = mesher;
            task.Execute();
            task.IsComplete = true;
            TerrainMeshRenderContainer renderContainer = task.Mesher.MeshDataContainer;
            Chunk newChunk = new()
            {
                LOD = task.LOD,
                Position = task.ChunkPosition,
                NeighborsMask = task.NeighborsMask,
                MinPosition = task.ChunkPositionMin
            };
            newChunk.TerrainMeshes.Add(ProcessTerrainMesh(renderContainer.MainData, $"Main {newChunk.Position}", newChunk));
            newChunk.TerrainMeshes.Add(ProcessTerrainMesh(renderContainer.LeftTransitionData, $"Left Transition {newChunk.Position}", newChunk));
            newChunk.TerrainMeshes.Add(ProcessTerrainMesh(renderContainer.DownTransitionData, $"Down Transition {newChunk.Position}", newChunk));
            newChunk.TerrainMeshes.Add(ProcessTerrainMesh(renderContainer.BackTransitionData, $"Back Transition {newChunk.Position}", newChunk));
            newChunk.TerrainMeshes.Add(ProcessTerrainMesh(renderContainer.RightTransitionData, $"Right Transition {newChunk.Position}", newChunk));
            newChunk.TerrainMeshes.Add(ProcessTerrainMesh(renderContainer.UpTransitionData, $"Up Transition {newChunk.Position}", newChunk));
            newChunk.TerrainMeshes.Add(ProcessTerrainMesh(renderContainer.ForwardTransitionData, $"Forward Transition {newChunk.Position}", newChunk));
            foreach (TerrainMesh mesh in newChunk.TerrainMeshes)
            {
                if (mesh.Vertices.Count == 0)
                {
                    continue;
                }
                mesh.Load();
                Console.WriteLine($"Loaded mesh {mesh.Name}");
                Engine.TerrainMeshes.Add(mesh);
            }
            WorldState.ChunkData.ChunkMap.Add(task.ChunkPosition, newChunk);
        }
    }

    public TerrainMesh ProcessTerrainMesh(TerrainMeshRenderData renderData, string name, in Chunk newChunk)
    {
        TerrainMesh meshToprocess = new(newChunk.MinPosition, name)
        {
            Vertices = renderData.Vertices,
            Normals = renderData.Normals,
            Indices = renderData.Indices
        };
        for (int i = 0; i < renderData.SecondaryVertices.Count; i++)
        {
            SecondaryVertexData secondaryVertex = renderData.SecondaryVertices[i];
            if ((secondaryVertex.VertexMask & newChunk.NeighborsMask) == secondaryVertex.VertexMask)
            {
                meshToprocess.Vertices[secondaryVertex.VertexIndex] = secondaryVertex.Position;
            }
        }
        List<uint> indexList = [];
        // Skip invalid triangles (when two vertices are approximately at the same position or the vertices are colinear)
        for (int i = 0; i < meshToprocess.Indices.Count; i += 3)
        {
            Vector3 v0 = meshToprocess.Vertices[(int)meshToprocess.Indices[i]];
            Vector3 v1 = meshToprocess.Vertices[(int)meshToprocess.Indices[i + 1]];
            Vector3 v2 = meshToprocess.Vertices[(int)meshToprocess.Indices[i + 2]];
            if (v0 == v1 || v0 == v2 || v1 == v2 || Vector3.Cross(v1 - v0, v2 - v0).Length < 0.0001f)
            {
                continue;
            }
            indexList.Add(meshToprocess.Indices[i]);
            indexList.Add(meshToprocess.Indices[i + 1]);
            indexList.Add(meshToprocess.Indices[i + 2]);
        }
        for (int i = 0; i < indexList.Count; i++)
        {
            meshToprocess.Indices[i] = indexList[i];
        }
        return meshToprocess;
    }
}
