using OpenTK.Mathematics;

namespace GraphicsPlayground.Util;

public static class MathUtil
{
    /// <summary>The absolute value of a vector.</summary>
    public static Vector3 VecAbs(float x, float y, float z) => new(Math.Abs(x), Math.Abs(y), Math.Abs(z));

    /// <summary>
    /// Returns the aspect ratio of the given width and height in the form 16:9 (1920x1080 for instance) using a vector2.
    /// </summary>
    public static Vector2i GetAspectRatio(int width, int height)
    {
        double aspect = (double)width / height;
        double tolerance = 1.0e-6;
        double h1 = 1, h2 = 0, k1 = 0, k2 = 1;
        double x = aspect;
        double y;
        int i = 0;
        do
        {
            double a = Math.Floor(x);
            double temp = h1;
            h1 = a * h1 + h2;
            h2 = temp;
            temp = k1;
            k1 = a * k1 + k2;
            k2 = temp;
            x = 1 / (x - a);
            y = h1 / k1;
            i++;
        }
        while (Math.Abs(aspect - y) > aspect * tolerance && i < 100);

        // Simplify the fraction
        long numerator = (long)Math.Round(h1);
        long denominator = (long)Math.Round(k1);

        return (Vector2i)new Vector2(numerator, denominator);
    }
}
