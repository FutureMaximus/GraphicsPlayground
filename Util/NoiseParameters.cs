namespace GraphicsPlayground.Util;

/// <summary> Parameters for a noise function. </summary>
public class NoiseParameters
{
    public int? Seed;
    public Noise.NoiseType? NoiseType;
    public Noise.RotationType3D? RotationType3D;
    public Noise.FractalType? FractalType;
    public Noise.CellularDistanceFunction? CellularDistanceFunction;
    public Noise.CellularReturnType? CellularReturnType;
    public int? NumberOfOctaves;
    public float? Amplitude;
    public float? Frequency;
    public float? Lacunarity;
    public Noise.DomainWarpType? DomainWarpType;
    public float? DomainWarpAmp;
    /// <summary> Needs to be between 0.0-1.0 </summary>
    public float? FractalWeightedStrength;
    public float? FractalPingPongStrength;
    /// <summary> Needs to be between 0.0-1.0 </summary>
    public float? CellularJitter;
}
