using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Models;

public sealed class Transformation
{
    public bool HasChanged = true;
    public Vector3 Position
    {
        get => _position;
        set
        {
            _position = value;
            HasChanged = true;
        }
    }
    public Quaternion Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            HasChanged = true;
        }
    }
    public Vector3 RotationEuler
    {
        get => _rotationEulerStorage;
        set
        {
            _rotationEulerStorage = value;
            Vector3 radians = new(MathHelper.DegreesToRadians(value.X), MathHelper.DegreesToRadians(value.Y), MathHelper.DegreesToRadians(value.Z));
            Rotation = Quaternion.FromEulerAngles(radians);
        }
    }
    private Vector3 _rotationEulerStorage = Vector3.Zero;
    public Vector3 Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            HasChanged = true;
        }
    }

    private Vector3 _position = Vector3.Zero;
    private Quaternion _rotation = Quaternion.Identity;
    private Vector3 _scale = Vector3.One;

    public Transformation()
    {

    }
    public Transformation(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }
    public Transformation(Matrix4 data)
    {
        Data = data;
    }

    public Matrix4 Data
    {
        get => HasChanged ? _data = Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(Position) : _data;
        set
        {
            _data = value;
            HasChanged = false;
        }
    }

    private Matrix4 _data = Matrix4.Identity;

    public static Matrix4 operator *(Transformation transformation, Matrix4 matrix)
    {
        return transformation.Data * matrix;
    }

    public static Transformation operator *(Transformation transformation, Transformation other)
    {
        return new(transformation.Data * other.Data);
    }
}
