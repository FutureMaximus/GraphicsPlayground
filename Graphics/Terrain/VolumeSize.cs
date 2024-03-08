namespace GraphicsPlayground.Graphics.Terrain;

/// <summary>
/// Based off of https://transvoxel.org/.
/// Lengyel, Eric. “Voxel-Based Terrain for Real-Time Virtual Simulations”. PhD diss., University of California at Davis, 2010.
/// </summary>
public sealed class VolumeSize
{
    /// <summary> The axis unit size of the volume. </summary>
    public readonly long SideLength = 8;
    /// <summary> The axis unit size^2 </summary>
    public readonly long SideLengthSquared = 64;
    /// <summary> The axis unit size^3 </summary>
    public readonly long SideLengthCubed = 512;

    public VolumeSize(long sideLength)
    {
        if (sideLength % 2 != 0)
            throw new ArgumentException("Side length not a power of 2", nameof(sideLength));
        if (sideLength < 8)
            throw new ArgumentException("Side length too small", nameof(sideLength));

        SideLength = sideLength;
        SideLengthSquared = sideLength * sideLength;
        SideLengthCubed = sideLength * sideLength * sideLength;
        if (SideLengthCubed > long.MaxValue)
            throw new ArgumentException("Side length too large", nameof(sideLength));
    }
}
