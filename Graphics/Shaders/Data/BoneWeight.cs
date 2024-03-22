using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsPlayground.Graphics.Shaders.Data;

///<summary>A struct representing a bone weight for a vertex.</summary>
public struct BoneWeight(int boneID, float weight)
{
    public int BoneID = boneID;
    public float Weight = weight;
}
