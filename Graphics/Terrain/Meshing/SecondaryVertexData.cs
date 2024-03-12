using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Terrain.Meshing;

public struct SecondaryVertexData
{
    public Vector3 Position;
    public ushort VertexMask;
    public ushort VertexIndex;
}
