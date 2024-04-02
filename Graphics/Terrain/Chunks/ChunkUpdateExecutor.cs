using GraphicsPlayground.Graphics.Terrain.World;
using GraphicsPlayground.Util;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Terrain.Chunks;

/// <summary>Provides chunk updates using an octree.</summary>
public class ChunkUpdateExecutor(WorldSettings worldSettings, ref WorldState worldState)
{
    /// <summary>The octree used for partitioning the chunks.</summary>
    public LinearOctree Octree = worldState.ChunkData.ChunkTree;
    ///<summary>All the queued chunk updates.</summary>
    public List<ChunkUpdate> ChunkUpdates = worldState.ChunkData.ChunkUpdates;
    /// <summary>The target position where this will generate around.</summary>
    public Vector3 TargetPosition;
    /// <summary>Currently active nodes in the octree.</summary>
    public HashSet<ulong> ActiveNodes = [];
    /// <summary>The neighbors of the active nodes.</summary>
    public Dictionary<Vector3i, int> ActiveNodesNeighbors = [];
    /// <summary>The map of chunk updates.</summary>
    public Dictionary<Vector3i, int> ChunkUpdatesMap = [];
    /// <summary>The map of chunk updates.</summary>
    public WorldSettings WorldSettings = worldSettings;

    /// <summary>Starts the chunk updater.</summary>
    public void Start()
    {
        TargetPosition = WorldSettings.TargetPosition;
    }

    /// <summary>Executes the chunk updater.</summary>
    public void Execute()
    {
        // TODO: Use distance from the target position to determine which chunks to update
        GetChunkUpdates(Octree.RootNode);
        BuildTransitionMasks();
        ChunkUpdates.Sort((x, y) =>
        {
            if (x.LOD < y.LOD)
            {
                return 1;
            }
            else if (x.LOD > y.LOD)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        });
    }

    /// <summary>Gets the chunk updates for the given node.</summary>
    public void GetChunkUpdates(OctreeNode fromNode)
    {
        if (CanRender(fromNode))
        {
            if (!ActiveNodes.Contains(fromNode.LocationCode))
            {
                ChunkUpdate chunkUpdate = new()
                {
                    UpdateType = ChunkUpdateType.Create,
                    Position = fromNode.Position,
                    LOD = fromNode.Depth
                };
                if (Octree.NodeHasChildren(fromNode))
                {
                    AddMergedLeavesUpdates(fromNode);
                }
                ChunkUpdates.Add(chunkUpdate);
                ChunkUpdatesMap.Add(chunkUpdate.Position, ChunkUpdates.Count - 1);
                ActiveNodes.Add(fromNode.LocationCode);
                ActiveNodesNeighbors.Add(chunkUpdate.Position, 0);
            }
        }
        else
        {
            if (ActiveNodes.Contains(fromNode.LocationCode))
            {
                ChunkUpdate chunkUpdate = new()
                {
                    UpdateType = ChunkUpdateType.Remove,
                    Position = fromNode.Position,
                    LOD = fromNode.Depth
                };
                ChunkUpdates.Add(chunkUpdate);
                ActiveNodes.Remove(fromNode.LocationCode);
                ActiveNodesNeighbors.Remove(fromNode.Position);
            }
            if (!Octree.NodeHasChildren(fromNode))
            {
                Octree.SplitNode(fromNode);
            }
            for (uint i = 0; i < 8; i++)
            {
                GetChunkUpdates(Octree.GetChild(fromNode, i));
            }
        }
    }

    /// <summary>Builds the transition masks for the chunk updates.</summary>
    public void BuildTransitionMasks()
    {
        Vector3i[] transitionDirections = GetTransitionDirections();
        IntBoundingBox worldBox = new(Octree.RootPosition, Octree.RootNode.Extents);
        for (int i = ChunkUpdates.Count - 1; i >= 0; i--)
        {
            ChunkUpdate chunkUpdate = ChunkUpdates[i];
            if (chunkUpdate.UpdateType == ChunkUpdateType.Remove)
            {
                continue;
            }
            int neighborsMask = 0;
            int halfLeafSize = Octree.LeafSize >> 1;
            for (int j = 0; j < transitionDirections.Length; j++)
            {
                Vector3i pos = chunkUpdate.Position + transitionDirections[j] * ((halfLeafSize << chunkUpdate.LOD) + 5);
                if (!worldBox.Contains(pos))
                {
                    continue;
                }
                OctreeNode otherNode = Octree.GetNodeAt(pos);
                int neighborBit = 1 << j;
                int neighborBitOpposite = ((neighborBit << 3) | (neighborBit >> 3)) & 0b111111;
                if (otherNode.Depth < chunkUpdate.LOD)
                {
                    neighborsMask |= neighborBit;
                }
                int desiredOtherNeighborBit = otherNode.Depth > chunkUpdate.LOD ? neighborBitOpposite : 0;
                int otherNeighborsMask = ActiveNodesNeighbors[otherNode.Position];
                int isolatedOtherNeighborBit = otherNeighborsMask & neighborBitOpposite;
                if (isolatedOtherNeighborBit != desiredOtherNeighborBit)
                {
                    otherNeighborsMask ^= neighborBitOpposite;
                    if (!ChunkUpdatesMap.TryGetValue(otherNode.Position, out int value))
                    {
                        ChunkUpdate transitionUpdate = new()
                        {
                            Position = otherNode.Position,
                            LOD = otherNode.Depth,
                            UpdateType = ChunkUpdateType.Update,
                            NeighborsMask = otherNeighborsMask
                        };
                        ChunkUpdates.Add(transitionUpdate);
                        ChunkUpdatesMap.Add(transitionUpdate.Position, ChunkUpdates.Count - 1);
                    }
                    else
                    {
                        int updateIndex = value;
                        ChunkUpdate otherNodeUpdate = ChunkUpdates[updateIndex];
                        otherNodeUpdate.NeighborsMask = otherNeighborsMask;
                        ChunkUpdates[updateIndex] = otherNodeUpdate;
                    }
                    ActiveNodesNeighbors[otherNode.Position] = otherNeighborsMask;
                }
            }
            ActiveNodesNeighbors[chunkUpdate.Position] = neighborsMask;
            chunkUpdate.NeighborsMask = neighborsMask;
            ChunkUpdates[i] = chunkUpdate;
        }
    }

    /// <summary>Adds merged leaves updates for the given node.</summary>

    public void AddMergedLeavesUpdates(OctreeNode fromNode)
    {
        for (uint i = 0; i < 8; i++)
        {
            OctreeNode child = Octree.GetChild(fromNode, i);
            if (Octree.NodeHasChildren(child))
            {
                AddMergedLeavesUpdates(child);
            }
            else
            {
                ChunkUpdate chunkUpdate = new()
                {
                    UpdateType = ChunkUpdateType.Remove,
                    Position = child.Position,
                    LOD = child.Depth
                };
                ChunkUpdates.Add(chunkUpdate);
                ActiveNodes.Remove(child.LocationCode);
                ActiveNodesNeighbors.Remove(child.Position);
            }
            if (!Octree.RemoveNode(child.LocationCode))
            {
                throw new Exception($"Failed to remove child node {child.LocationCode}");
            }
        }
    }

    /// <summary>Returns all 6 transition directions used for "stiching" together chunks</summary>
    public static Vector3i[] GetTransitionDirections()
    {
        Vector3i[] directions = new Vector3i[6];
        for (int i = 0; i < 6; i++)
        {
            int idx = 1 << i;
            directions[i] = new Vector3i(
                (idx & 1) * -1 + ((idx & 8) >> 3) * 1,
                ((idx & 2) >> 1) * -1 + ((idx & 16) >> 4) * 1,
                ((idx & 4) >> 2) * -1 + ((idx & 32) >> 5) * 1);
        }
        return directions;
    }


    /// <summary>Checks if the node can be rendered based on the distance from the target position using the octree this is not culling distance.</summary>
    public bool CanRender(OctreeNode node)
    {
        if (node.Depth == 0)
        {
            return true;
        }
        Vector3 nodePosition = node.Position;
        float distX = Math.Abs(nodePosition.X - TargetPosition.X);
        float distY = Math.Abs(nodePosition.Y - TargetPosition.Y);
        float distZ = Math.Abs(nodePosition.Z - TargetPosition.Z);
        float minDist = Math.Max(distX, Math.Max(distY, distZ));
        float compareDist = Octree.LeafSize * 1.5f * (1 << node.Depth);
        return minDist > compareDist;
    }
}
