namespace GraphicsPlayground.Util;

/// <summary> A symetric matrix. https://en.wikipedia.org/wiki/Symmetric_matrix </summary>
public class SymmetricMatrix
{
    public readonly double[] M = new double[10];

    public SymmetricMatrix(double c = 0)
    {
        for (int i = 0; i < 10; i++)
        {
            M[i] = c;
        }
    }

    public SymmetricMatrix(double m11, double m12, double m13, double m14,
                           double m22, double m23, double m24,
                           double m33, double m34, double m44)
    {
        M[0] = m11; M[1] = m12; M[2] = m13; M[3] = m14;
        M[4] = m22; M[5] = m23; M[6] = m24;
        M[7] = m33; M[8] = m34;
        M[9] = m44;
    }

    // Make plane
    public SymmetricMatrix(double a, double b, double c, double d)
    {
        M[0] = a * a; M[1] = a * b; M[2] = a * c; M[3] = a * d;
        M[4] = b * b; M[5] = b * c; M[6] = b * d;
        M[7] = c * c; M[8] = c * d;
        M[9] = d * d;
    }

    /// <summary> The determinant of the matrix. </summary>
    public double Det(int a11, int a12, int a13,
                      int a21, int a22, int a23,
                      int a31, int a32, int a33)
    {
        return M[a11] * M[a22] * M[a33] + M[a13] * M[a21] * M[a32] + M[a12] * M[a23] * M[a31]
                    - M[a13] * M[a22] * M[a31] - M[a11] * M[a23] * M[a32] - M[a12] * M[a21] * M[a33];
    }

    public static SymmetricMatrix operator +(in SymmetricMatrix m, in SymmetricMatrix n)
    {
        return new SymmetricMatrix(m[0] + n[0], m[1] + n[1], m[2] + n[2], m[3] + n[3],
                                                m[4] + n[4], m[5] + n[5], m[6] + n[6],
                                                m[7] + n[7], m[8] + n[8], m[9] + n[9]);
    }

    public double this[int c]
    {
        get => M[c];
    }
}
