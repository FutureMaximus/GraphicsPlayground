using System.Collections.Concurrent;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Util;

/// <summary>Linear octree data structure.</summary>
public struct LinearOctree
{
    public ConcurrentDictionary<ulong, OctreeNode> NodeMap = new();
    public Vector3i RootPosition;
    public int RootSize;
    public int RootDepth;
    public int LeafSize;

    public LinearOctree(Vector3i rootPos, int rootSize, int rootDepth)
    {
        RootPosition = rootPos;
        RootSize = rootSize;
        RootDepth = rootDepth;
        LeafSize = RootSize >> RootDepth;

        CreateRootNode();
    }

    /// <summary>Creates the root node of the octree.</summary>
    public void CreateRootNode()
    {
        if (NodeMap.ContainsKey(1))
        {
            throw new ArgumentException("Root node already exists");
        }
        NodeMap[1] = new()
        {
            Depth = RootDepth,
            LocationCode = 1,
            Extents = RootSize / 2
        };
    }

    /// <summary>Returns the root node of the octree.</summary>
    public readonly OctreeNode RootNode => NodeMap[1];

    /// <summary>Returns whether the node has children.</summary>
    public readonly bool NodeHasChildren(OctreeNode node) => NodeMap.ContainsKey(node.LocationCode << 3);

    /// <summary>Returns the child of the node at the given index.</summary>
    public readonly OctreeNode GetChild(OctreeNode node, uint index) => NodeMap[(node.LocationCode << 3) | index];

    /// <summary>Returns the node at the given position.</summary>
    public readonly OctreeNode GetNodeAt(Vector3 position)
    {
        OctreeNode currentNode = RootNode;
        while (NodeHasChildren(currentNode))
        {
            uint index = ((position.X > currentNode.Position.X) ? 1u : 0u)
                       + ((position.Y > currentNode.Position.Y) ? 2u : 0u)
                       + ((position.Z > currentNode.Position.Z) ? 4u : 0u);
            currentNode = GetChild(currentNode, index);
        }
        return currentNode;
    }

    /// <summary>Removes the node at the given location code.</summary>
    public readonly bool RemoveNode(ulong locationCode) => NodeMap.Remove(locationCode, out _);

    /// <summary>Splits a node</summary>
    public readonly void SplitNode(OctreeNode node)
    {
        if (NodeMap.ContainsKey(node.LocationCode << 3))
        {
            throw new ArgumentException("Node already has children");
        }
        if (node.Depth == 0)
        {
            throw new ArgumentException("Cannot split as the node is already a leaf");
        }
        int childDepth = node.Depth - 1;
        int childExtents = node.Extents >> 1;
        for (uint i = 0; i < 8; i++)
        {
            OctreeNode child;
            child.Depth = childDepth;
            child.Extents = childExtents;
            child.LocationCode = (node.LocationCode << 3) | i;
            child.Position = node.Position + new Vector3i(
                childExtents * ((i & 1) > 0 ? 1 : -1),
                childExtents * ((i & 2) > 0 ? 1 : -1),
                childExtents * ((i & 4) > 0 ? 1 : -1));

            NodeMap.TryAdd(child.LocationCode, child);
        }
    }
}
