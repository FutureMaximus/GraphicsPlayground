using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Render;

public class Camera
{
    public Vector3 Position = Vector3.Zero;
    public Vector3 Direction = -Vector3.UnitZ;
    public Vector3 Up = Vector3.UnitY;
    public Vector3 Side
    {
        get => Vector3.Cross(Direction, Up).Normalized();
    }

    public float Pitch
    {
        get => MathHelper.RadiansToDegrees(_pitch);
        set
        {
            _pitch = MathHelper.DegreesToRadians(value);
            UpdateVectors();
        }
    }

    public float Yaw
    {
        get => MathHelper.RadiansToDegrees(_yaw);
        set
        {
            _yaw = MathHelper.DegreesToRadians(value);
            UpdateVectors();
        }
    }

    public Matrix4 View
    {
        get => Matrix4.LookAt(Position, Position + Direction, Up.Normalized());
    }

    /// <summary>
    /// Returns the squared distance to the given poin. This should be used for comparisons
    /// as it is faster than the normal distance calculation.
    /// </summary>
    /// <param name="point"></param>
    /// <returns>
    /// The squared distance to the given point.
    /// </returns>
    public float SquaredDistanceTo(Vector3 point)
    {
        return (Position - point).LengthSquared;
    }

    private float _pitch = 0;

    private float _yaw = 0;

    public void MoveForward(float deltaTime, float speed) => Position += Direction * speed * deltaTime;

    public void MoveBackward(float deltaTime, float speed) => Position -= Direction * speed * deltaTime;

    public void MoveLeft(float deltaTime, float speed) => Position -= Side * speed * deltaTime;

    public void MoveRight(float deltaTime, float speed) => Position += Side * speed * deltaTime;

    public void MoveUp(float deltaTime, float speed) => Position += Up * speed * deltaTime;

    public void MoveDown(float deltaTime, float speed) => Position -= Up * speed * deltaTime;

    public void UpdateVectors()
    {
        Vector3 direction = new Vector3(
                (float)Math.Cos(MathHelper.DegreesToRadians(Yaw)) * (float)Math.Cos(MathHelper.DegreesToRadians(Pitch)),
                (float)Math.Sin(MathHelper.DegreesToRadians(Pitch)),
                (float)Math.Sin(MathHelper.DegreesToRadians(Yaw)) * (float)Math.Cos(MathHelper.DegreesToRadians(Pitch))
        ).Normalized(); // Normalize the vector, because its length gets closer to 0 the more you look up or down which results in slower movement.
        Direction = direction;
    }
}
