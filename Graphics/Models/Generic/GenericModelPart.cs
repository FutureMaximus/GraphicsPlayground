using GraphicsPlayground.Graphics.Models.Mesh;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Models.Generic;

/// <summary> Generic model part data also known as a bone with renderable meshes. </summary>
public class GenericModelPart(string name, IModel coreModel) : IModelPart, IDisposable
{
    // ====== Part Data ======
    /// <summary> The core model that owns this part </summary>
    public IModel CoreModel = coreModel;
    /// <summary> The parent of this part </summary>
    public IModelPart? Parent { get; set; }
    /// <summary> The children of this part </summary>
    private readonly HashSet<GenericModelPart> _children = [];
    public void AddChild(GenericModelPart child)
    {
        _children.Add(child);
        child.Parent = this;
    }
    public void RemoveChild(GenericModelPart child)
    {
        _children.Remove(child);
        child.Parent = null;
    }
    public HashSet<GenericModelPart> GetChildren() => _children;
    public List<GenericMesh> Meshes = [];
    public BufferUsageHint ModelUsageHint => CoreModel.ModelUsageHint;
    public string Name = name;
    public Guid ID => _id;
    private readonly Guid _id = Guid.NewGuid();

    // ====== Transform Data ======
    /// <summary> The transformation of the model part relative to the parent. </summary>
    public Transformation LocalTransformation = new();
    /// <summary> The transformation of the model part used for rendering. </summary>
    public Matrix4 Transformation => LocalTransformation * (Parent?.Transformation ?? Matrix4.Identity) * CoreModel.Transformation();

    /// <summary> Returns the normal matrix for this model part</summary>
    public Matrix3 NormalMatrix()
    {
        Matrix4 transposeInv = Matrix4.Transpose(Transformation.Inverted());
        return new(
        transposeInv.M11, transposeInv.M12, transposeInv.M13,
        transposeInv.M21, transposeInv.M22, transposeInv.M23,
        transposeInv.M31, transposeInv.M32, transposeInv.M33);
    }
    // ============================

    public void Load()
    {
        foreach (GenericMesh mesh in Meshes)
        {
            mesh.Load();
        }
    }

    public void Dispose()
    {
        foreach (GenericMesh mesh in Meshes)
        {
            mesh.Dispose();
        }
        GC.SuppressFinalize(this);
    }

    public override bool Equals(object? obj)
    {
        if (obj is GenericModelPart part)
        {
            return part.ID == ID;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ID);
    }

    public object Clone()
    {
        GenericModelPart part = new(Name, CoreModel)
        {
            LocalTransformation = LocalTransformation
        };
        foreach (GenericMesh mesh in Meshes)
        {
            // Do not clone mesh data as it is shared between all instances of the model
            part.Meshes.Add(mesh);
        }
        return part;
    }
}
