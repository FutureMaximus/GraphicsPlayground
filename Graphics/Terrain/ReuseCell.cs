using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsPlayground.Graphics.Terrain;

public class ReuseCell
{
    public readonly int[] Verts;

    public ReuseCell(int size)
    {
        Verts = new int[size];

        for (int i = 0; i < size; i++)
            Verts[i] = -1;
    }
}
