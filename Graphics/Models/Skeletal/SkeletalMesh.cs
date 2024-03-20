using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsPlayground.Graphics.Models.Skeletal;

public class SkeletalMesh(string name, SkeletalModelPart modelPart) : IDisposable
{
    public void Dispose()
    {
        // TODO: Dispose GPU resources.
        GC.SuppressFinalize(this);
    }
}
