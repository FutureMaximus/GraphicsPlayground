using GraphicsPlayground.Graphics.Models.Generic;
using GraphicsPlayground.Graphics.Models.Mesh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsPlayground.Graphics.Models;

public class Model(string name)
{
    /// <summary> The name of the model. </summary>
    public string Name = name;

    /// <summary> The transformation of the model. </summary>
    public Transformation Transformation = new();

    /// <summary> The render data of the model. </summary>
    public ModelRenderData RenderData = new();

    /// <summary> The parts of the model. </summary>
    public List<ModelPart> Parts = [];

    /// <summary> The unique identifier of the model. </summary>
    public Guid ID => _id;
    private readonly Guid _id = Guid.NewGuid();

    ///<summary>Loads the model parts and sends their mesh data to the GPU.</summary>
    public void Load()
    {
        foreach (ModelPart part in Parts)
        {
            part.Load();
        }
    }

    ///<summary>Renders the model parts.</summary>
    public void Render()
    {
        foreach (ModelPart part in Parts)
        {
            foreach (IMesh mesh in part.Meshes)
            {
                mesh.Render();
            }
        }
    }

    ///<summary>Unloads the model parts from the GPU.</summary>
    public void Unload()
    {
        foreach (ModelPart part in Parts)
        {
            foreach (IMesh mesh in part.Meshes)
            {
                mesh.Dispose();
            }
        }
    }

    public void Dispose()
    {
        foreach (ModelPart part in Parts)
        {
            part.Dispose();
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is GenericModel model)
        {
            return model.ID == ID;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ID);
    }
}
