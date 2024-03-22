using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsPlayground.Graphics.Models.Skeletal;

public class SkeletalModelPart(string name, IModel coreModel)
{
    public string Name = name;
    public IModel CoreModel = coreModel;
    public BufferUsageHint ModelUsageHint => CoreModel.ModelUsageHint;

    public IModelPart? Parent { get; set; }
}
