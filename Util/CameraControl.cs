using GraphicsPlayground.Graphics.Render;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GraphicsPlayground.Util;

public class CameraControl(Camera camera)
{
    public static bool CanMove
    {
        get => _canMove;
        set
        {
            if (value)
            {
                _canMove = true;
            }
            else
            {
                _canMove = false;
            }
        }
    }
    private static bool _canMove = true;

    public float Speed
    {
        get => _speed;
        set
        {
            if (value < 0)
            {
                _speed = 0;
            }
            else
            {
                _speed = value;
            }
        }
    }
    public bool IsMoving = false;

    private readonly Camera _camera = camera;
    private float _speed = 10.0f;

    public void UpdateKeyboardState(KeyboardState keyboard, float deltaTime)
    {
        if (!CanMove)
        {
            return;
        }
        IsMoving = false;
        if (keyboard.IsKeyDown(Keys.LeftShift))
        {
            IsMoving = true;
        }

        if (!IsMoving) return;

        if (keyboard.IsKeyDown(Keys.W))
        {
            _camera.MoveForward(deltaTime, Speed);
        }
        if (keyboard.IsKeyDown(Keys.S))
        {
            _camera.MoveBackward(deltaTime, Speed);
        }
        if (keyboard.IsKeyDown(Keys.A))
        {
            _camera.MoveLeft(deltaTime, Speed);
        }
        if (keyboard.IsKeyDown(Keys.D))
        {
            _camera.MoveRight(deltaTime, Speed);
        }
        if (keyboard.IsKeyDown(Keys.Space))
        {
            _camera.MoveUp(deltaTime, Speed);
        }
        if (keyboard.IsKeyDown(Keys.LeftControl))
        {
            _camera.MoveDown(deltaTime, Speed);
        }
    }

    public void UpdateMouseState(MouseState mouse)
    {
        if (!CanMove)
        {
            return;
        }
        float deltaX = mouse.Delta.X;
        float deltaY = mouse.Delta.Y;
        _camera.Yaw += deltaX * Config.Settings.MouseSensitivity.X;
        _camera.Pitch -= deltaY * Config.Settings.MouseSensitivity.Y;
        _camera.Pitch = Math.Clamp(_camera.Pitch, -89.0f, 89.0f);
        _camera.UpdateVectors();

        if (IsMoving)
        {
            Speed += mouse.ScrollDelta.Y * 0.1f;
        }
        else
        {
            _camera.Position += _camera.Direction * mouse.ScrollDelta.Y * 0.1f;
        }
    }
}
