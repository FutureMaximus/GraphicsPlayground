using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Models;

///<summary>A part of a model that can contain other parts in a parent-child relationship.</summary>
public class ModelPart(string name, Model coreModel) : IDisposable
{
    ///<summary>The name of this part.</summary>
    public string Name = name;
    ///<summary>The core model of this part.</summary>
    public Model CoreModel = coreModel;
    ///<summary>The parent of this part.</summary>
    public ModelPart? Parent { get; set; }

    /// <summary>Adds a child part to the model and assigns the parent.</summary>
    public void AddChild(ModelPart child)
    {
        _children.Add(child);
        child.Parent = this;
    }
    /// <summary>Removes a child from the model.</summary>
    public void RemoveChild(ModelPart child)
    {
        _children.Remove(child);
        child.Parent = null;
    }
    ///<summary>Returns the children of this part.</summary>
    public HashSet<ModelPart> GetChildren() => _children;
    private readonly HashSet<ModelPart> _children = [];

    /// <summary>The meshes associated with this part.</summary>
    public List<IMesh> Meshes = [];
    ///<summary>The local transformation of this part.</summary>
    public Transformation LocalTransformation = new();
    ///<summary>The transformation of this part.</summary>
    public Matrix4 Transformation => LocalTransformation * (Parent?.Transformation ?? Matrix4.Identity) * CoreModel.Transformation;
    ///<summary>The normal matrix of this part.</summary>
    public Matrix3 NormalMatrix()
    {
        Matrix4 transposeInv = Matrix4.Transpose(Transformation.Inverted());
        return new(
        transposeInv.M11, transposeInv.M12, transposeInv.M13,
        transposeInv.M21, transposeInv.M22, transposeInv.M23,
        transposeInv.M31, transposeInv.M32, transposeInv.M33);
    }

    public void Load()
    {
        foreach (IMesh mesh in Meshes)
        {
            mesh.Load();
        }
    }

    ///<summary>Disposes of the part and its children.</summary>
    public void Dispose()
    {
        foreach (IMesh mesh in Meshes)
        {
            mesh.Dispose();
        }
        foreach (ModelPart part in _children)
        {
            part.Dispose();
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>Clones the model part.</summary>
    public object Clone()
    {
        ModelPart part = new(Name, CoreModel)
        {
            LocalTransformation = LocalTransformation
        };
        foreach (IMesh mesh in Meshes)
        {
            // Do not clone mesh data as it is shared between all instances of the model
            part.Meshes.Add(mesh);
        }
        return part;
    }
}
