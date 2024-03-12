using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsPlayground.Graphics.Terrain.Meshing;

public struct MeshTaskData
{
    public Vector3i ChunkPosition;
    public Vector3i ChunkPositionMin;
    public int LOD;
    public int NeighborsMask;
}
