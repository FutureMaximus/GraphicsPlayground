using OpenTK.Mathematics;
using GraphicsPlayground.Util;

namespace GraphicsPlayground.Graphics.Models.MarchingCubes;

/// <summary> 
/// Signed distance functions for use with the marching cubes algorithm. 
/// See <see cref="MarchingCube"/> and <see cref="IsoSurface"/>. 
/// Referenced from https://iquilezles.org/articles/distfunctions/.
/// </summary>
public static class SignedDistanceFunctions
{
    public static Func<Vector3, float> Sphere(Vector3 pos, float radius) => (p) => (p - pos).Length - radius;

    public static Func<Vector3, float> Box(Vector3 size) => (p) =>
    {
        Vector3 q = MathUtil.VecAbs(p.X, p.Y, p.Z) - size;
        return q.Length;
    };

    public static Func<Vector3, float> RoundBox(Vector3 size, float radius) => (p) =>
    {
        Vector3 q = MathUtil.VecAbs(p.X, p.Y, p.Z) - size;
        return q.Length - radius;
    };

    public static Func<Vector3, float> Torus(float radius1, float radius2) => (p) =>
    {
        Vector2 q = new(p.X, p.Z);
        return (q.Length - radius1) * (q.Length - radius1) + p.Y * p.Y - radius2 * radius2;
    };

    public static Func<Vector3, float> Cylinder(float radius, float height) => (p) =>
    {
        Vector2 d = new(p.X, p.Z);
        return MathF.Max(d.Length - radius, MathF.Abs(p.Y) - height);
    };

    public static Func<Vector3, float> Cone(float radius, float height) => (p) =>
    {
        Vector2 q = new(p.X, p.Z);
        float qlen = q.Length;
        return MathF.Max(qlen - radius, p.Y - height);
    };

    public static Func<Vector3, float> Plane(Vector3 n, float h) => (p) => Vector3.Dot(p, n) + h;

    public static Func<Vector3, float> HexagonalPrism(Vector2 size) => (p) =>
    {
        Vector3 q = new(p.X, p.Y * 0.866025404f, p.Z * 0.5f);
        Vector3 r = new(MathF.Abs(q.X) - size.X, q.Y - size.Y, MathF.Abs(q.Z) - size.X);
        Vector3 s = new(MathF.Max(r.X, 0), MathF.Max(r.Y, 0), MathF.Max(r.Z, 0));
        float a = MathF.Min(MathF.Max(r.X, MathF.Max(r.Y, r.Z)), 0.0f);
        return a + s.Length;
    };

    public static Func<Vector3, float> TriangularPrism(Vector2 size) => (p) =>
    {
        Vector3 q = new(MathF.Abs(p.X), MathF.Abs(p.Y), MathF.Abs(p.Z));
        return MathF.Max(q.Z - size.Y, MathF.Max(q.X * 0.866025404f + p.Y * 0.5f, -p.Y) - size.X * 0.5f);
    };

    public static Func<Vector3, float> Capsule(Vector3 p0, Vector3 p1, float r) => (p) =>
    {
        Vector3 pa = p - p0;
        Vector3 ba = p1 - p0;
        float h = Math.Clamp(Vector3.Dot(pa, ba) / Vector3.Dot(ba, ba), 0.0f, 1.0f);
        return (pa - ba * h).Length - r;
    };

    public static Func<Vector3, float> CappedCylinder(float h, float r1, float r2) => (p) =>
    {
        Vector2 d = new(p.X, p.Z);
        return MathF.Max(MathF.Abs(p.Y) - h, d.Length - MathHelper.Lerp(r1, r2, p.Y / h));
    };

    public static Func<Vector3, float> CappedCone(float h, float r1, float r2) => (p) =>
    {
        Vector2 d = new(p.X, p.Z);
        Vector2 q = new(d.Length, p.Y);
        Vector2 v = new(r2 - r1, 2 * h);
        Vector2 w = new(q.X - r1, q.Y);
        w -= v * Math.Clamp(Vector2.Dot(v, w) / Vector2.Dot(v, v), 0.0f, 1.0f);
        if (w.X < 0.0f) return w.Length;
        if (w.Y < 0.0f) return w.X;
        return w.Length;
    };
}
