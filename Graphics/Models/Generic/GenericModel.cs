using GraphicsPlayground.Graphics.Animations;
using GraphicsPlayground.Util;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Models.Generic;

/// <summary> A generic model class for custom made models. </summary>
public class GenericModel(string name) : IModel
{
    /// <summary> The render data of the model. </summary>
    public ModelRenderData ModelRenderData = new();

    /// <summary>
    /// The usage hint of the model
    /// if it is static use StaticDraw
    /// and if it is dynamic use DynamicDraw.
    /// </summary>
    public BufferUsageHint ModelUsageHint = BufferUsageHint.StaticDraw;

    public Vector3 Position = Vector3.Zero;
    public Quaternion Rotation = Quaternion.Identity;
    public Vector3 Scale = Vector3.One;

    public Matrix4 Transformation()
    {
        Matrix4 translate = Matrix4.CreateTranslation(Position);
        translate.Transpose();
        Matrix4 rotate = Matrix4.CreateFromQuaternion(Rotation);
        rotate.Transpose();
        Matrix4 scale = Matrix4.CreateScale(Scale);
        scale.Transpose();
        return translate * rotate * scale;
    }

    /// <summary> The root model parts that can contain children parts. </summary>
    public List<GenericModelPart> Parts = [];

    /// <summary> The bones associated with the model. </summary>
    public Dictionary<string, BoneInfo> Bones = [];
    /// <summary> The bone counter. </summary>
    public int BoneCounter = 0;

    /// <summary> The unique identifier of the model see <see cref="IIdentifiable"/>. </summary>
    public Guid ID => _id;
    private readonly Guid _id = Guid.NewGuid();

    /// <summary> The name of the model. </summary>
    public string Name { get => _name; set => _name = value ?? throw new ArgumentNullException(nameof(value)); }
    private string _name = name;

    public void Load()
    {
        foreach (GenericModelPart part in Parts)
        {
            part.Load();
        }
    }

    public void Render()
    {
        foreach (GenericModelPart part in Parts)
        {
            foreach (GenericMesh mesh in part.Meshes)
            {
                mesh.Render();
            }
        }
    }

    public void Unload()
    {
        foreach (GenericModelPart part in Parts)
        {
            foreach (GenericMesh mesh in part.Meshes)
            {
                mesh.Dispose();
            }
        }
    }

    public GenericModelPart? GetPartByGuid(Guid guid)
    {
        foreach (GenericModelPart part in Parts)
        {
            if (part.ID == guid)
            {
                return part;
            }
        }
        return null;
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

    public void Dispose()
    {
        foreach (GenericModelPart part in Parts)
        {
            part.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
