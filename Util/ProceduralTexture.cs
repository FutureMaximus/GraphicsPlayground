using OpenTK.Mathematics;
using System.Drawing;

namespace GraphicsPlayground.Util;

/// <summary>
/// Class for generating procedurally generated textures for PBR materials.
/// </summary>
public static class ProceduralTexture
{
    /// <summary>
    /// Procedural ARM materials.
    /// For the height range the values are between 0 and 255.
    /// For the chance the values are between 0 and 100.
    /// For the material values they are between 0 and 1.
    /// For the color range the values are between 0 and 255.
    /// </summary>
    public struct ProceduralARMMaterials
    {
        public List<ProceduralAmbientOcclusion> AmbientOcclusions;
        public List<ProceduralRoughness> Roughnesses;
        public List<ProceduralMetallic> Metallics;
    }

    public struct ProceduralAmbientOcclusion
    {
        public HeightRange HeightMapRange;
        public int Chance;
        public float Value;
    }

    public struct ProceduralRoughness
    {
        public ColorRange ColorRange;
        public int Chance;
        public float Value;
    }

    public struct ProceduralMetallic
    {
        public ColorRange ColorRange;
        public int Chance;
        public float Value;
    }

    public class HeightRange(float min, float max)
    {
        public float Min = min;
        public float Max = max;

        public bool IsInRange(float value)
        {
            return value >= Min && value <= Max;
        }
    }

    public class ColorRange(Color color, float threshold)
    {
        public Color Color = color;
        /// <summary> The threshold for the color range (0-255). </summary>
        public float Threshold = threshold;

        public bool IsInRange(Color value)
        {
            return Math.Abs(value.R - Color.R) <= Threshold &&
                Math.Abs(value.G - Color.G) <= Threshold &&
                Math.Abs(value.B - Color.B) <= Threshold;
        }
    }

    // TODO: Use ImageSharp instead of System.Drawing
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    /// <summary> Generates a height map from noise. </summary>
    public static Bitmap GenerateNoiseTexture(int height, int width, NoiseParameters noiseParams)
    {
        Bitmap proceduralBitmap = new(width, height);
        Noise noise = new();
        noise.SetNoiseType(Noise.NoiseType.Cellular);
        noise.SetCellularReturnType(Noise.CellularReturnType.Distance2Mul);
        noise.SetRotationType3D(Noise.RotationType3D.ImproveXZPlanes);
        noise.SetFractalType(Noise.FractalType.DomainWarpIndependent);
        //noise.SetFrequency(octave.Frequency);
        //noise.SetDomainWarpAmp(octave.Amplitude);
        //noise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2);
        noise.SetFractalOctaves((int)noiseParams.NumberOfOctaves);
        //noise.SetFractalLacunarity(octave.Lacunarity);
        //noise.SetFractalGain(octave.Persistence);
        Random rand = new();
        noise.SetSeed(rand.Next(1, 100000));

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                double n = noise.GetNoise(x, y);

                // Convert noise to gray scale color
                byte color = (byte)(n * 128 + 128);

                proceduralBitmap.SetPixel(x, y, Color.FromArgb(color, color, color));
            }
        }

        return proceduralBitmap;
    }

    /// <summary>
    /// Converts a value from one range to another like 0.0-1.0 to 0.0-255.0.
    /// </summary>
    /// <param name="originalStart"></param>
    /// <param name="originalEnd"></param>
    /// <param name="newStart"></param>
    /// <param name="newEnd"></param>
    /// <param name="value"></param>
    /// <returns>
    /// The converted value.
    /// </returns>

    public static float ConvertValueFromRange(float originalStart, float originalEnd, float newStart, float newEnd, float value)
    {
        double scale = (double)(newEnd - newStart) / (originalEnd - originalStart);
        return (float)(newStart + (value - originalStart) * scale);
    }

    public static Color[] GetRandomColors(int count)
    {
        Color[] colors = new Color[count];
        Random rand = new();
        for (int i = 0; i < count; i++)
        {
            colors[i] = Color.FromArgb(rand.Next(0, 255), rand.Next(0, 255), rand.Next(0, 255));
        }
        return colors;
    }

    /// <summary>
    /// Generates the albedo map from a height map
    /// </summary>
    /// <param name="heightMap"></param>
    /// <returns>
    /// The albedo map.
    /// </returns>
    /// 
    public static Bitmap GenerateAlbedoFromHeightMap(Bitmap heightMap, Color[] colors)
    {
        int width = heightMap.Width;
        int height = heightMap.Height;

        Bitmap albedoMap = new(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float heightValue = GetHeightValue(heightMap, x, y);

                // Map the height value to the color gradient
                int colorIndex = (int)(heightValue * (colors.Length - 1));
                Color color = colors[colorIndex];

                albedoMap.SetPixel(x, y, color);
            }
        }

        return albedoMap;
    }

    /// <summary>
    /// Generates an ARM map from an albedo map, height map, and <see cref="ProceduralARMMaterials"/>
    /// </summary>
    public static Bitmap GenerateARMMapFromMaterials(Bitmap albedoMap, Bitmap heightmap, ProceduralARMMaterials materials)
    {
        Bitmap armMap = new(albedoMap.Width, albedoMap.Height);
        for (int x = 0; x < albedoMap.Width; x++)
        {
            for (int y = 0; y < albedoMap.Height; y++)
            {
                Color albedoColor = albedoMap.GetPixel(x, y);
                float heightValue = GetHeightValue(heightmap, x, y);
                heightValue = ConvertValueFromRange(0.0f, 1.0f, 0.0f, 255.0f, heightValue);
                int r = GetRValueFromProceduralAOs(heightValue, materials.AmbientOcclusions);
                int g = GetGValueFromProceduralRoughnesses(albedoColor, materials.Roughnesses);
                int b = GetBValueFromProceduralMetallic(albedoColor, materials.Metallics);
                armMap.SetPixel(x, y, Color.FromArgb(r, g, b));
            }
        }
        return armMap;
    }

    private static int GetRValueFromProceduralAOs(float heightValue, List<ProceduralAmbientOcclusion> aoMats)
    {
        List<ProceduralAmbientOcclusion> aoMatsInRange = [];
        for (int i = 0; i < aoMats.Count; i++)
        {
            ProceduralAmbientOcclusion aoMat = aoMats[i];
            if (aoMat.HeightMapRange.IsInRange(heightValue))
            {
                aoMatsInRange.Add(aoMat);
            }
        }
        Random rand = new();
        for (int i = 0; i < aoMatsInRange.Count; i++)
        {
            ProceduralAmbientOcclusion aoMat = aoMatsInRange[i];
            if (rand.Next(0, 100) <= aoMat.Chance)
            {
                return (int)(aoMat.Value * 255);
            }
        }
        return 0;
    }

    private static int GetGValueFromProceduralRoughnesses(Color color, List<ProceduralRoughness> roughnessMats)
    {
        List<ProceduralRoughness> roughnessMatsInRange = new();
        for (int i = 0; i < roughnessMats.Count; i++)
        {
            ProceduralRoughness roughnessMat = roughnessMats[i];
            if (roughnessMat.ColorRange.IsInRange(color))
            {
                roughnessMatsInRange.Add(roughnessMat);
            }
        }
        Random rand = new();
        for (int i = 0; i < roughnessMatsInRange.Count; i++)
        {
            ProceduralRoughness roughnessMat = roughnessMatsInRange[i];
            if (rand.Next(0, 100) <= roughnessMat.Chance)
            {
                return (int)(roughnessMat.Value * 255);
            }
        }
        return 0;
    }

    private static int GetBValueFromProceduralMetallic(Color color, List<ProceduralMetallic> metallicMats)
    {
        List<ProceduralMetallic> metallicMatsInRange = new();
        for (int i = 0; i < metallicMats.Count; i++)
        {
            ProceduralMetallic metallicMat = metallicMats[i];
            if (metallicMat.ColorRange.IsInRange(color))
            {
                metallicMatsInRange.Add(metallicMat);
            }
        }
        Random rand = new();
        for (int i = 0; i < metallicMatsInRange.Count; i++)
        {
            ProceduralMetallic metallicMat = metallicMatsInRange[i];
            if (rand.Next(0, 100) <= metallicMat.Chance)
            {
                return (int)(metallicMat.Value * 255);
            }
        }
        return 0;
    }

    /// <summary>
    /// Generates a normal map from a height map using the Sobel operator.
    /// https://en.wikipedia.org/wiki/Sobel_operator
    /// </summary>
    public static Bitmap GenerateNormalFromHeightMap(Bitmap heightMap)
    {
        int width = heightMap.Width;
        int height = heightMap.Height;

        Bitmap normalMap = new(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Calculate the gradient using the Sobel operator
                float dX = GetSobelGradientX(heightMap, x, y);
                float dY = GetSobelGradientY(heightMap, x, y);

                // Compute the normal vector from the gradient
                Vector3 normal = new(-dX, -dY, 1.0f);
                normal.Normalize();

                // Encode the normal into RGB values
                int r = (int)((normal.X * 0.5f + 0.5f) * 255);
                int g = (int)((normal.Y * 0.5f + 0.5f) * 255);
                int b = (int)((normal.Z * 0.5f + 0.5f) * 255);

                normalMap.SetPixel(x, y, Color.FromArgb(r, g, b));
            }
        }

        return normalMap;
    }

    // Info on gradients https://en.wikipedia.org/wiki/Gradient

    // Helper function to calculate the Sobel gradient in the X direction
    private static float GetSobelGradientX(Bitmap heightMap, int x, int y)
    {
        float dX = 0.0f;

        dX += -1.0f * GetHeightValue(heightMap, x - 1, y - 1);
        dX += -2.0f * GetHeightValue(heightMap, x - 1, y);
        dX += -1.0f * GetHeightValue(heightMap, x - 1, y + 1);

        dX += 1.0f * GetHeightValue(heightMap, x + 1, y - 1);
        dX += 2.0f * GetHeightValue(heightMap, x + 1, y);
        dX += 1.0f * GetHeightValue(heightMap, x + 1, y + 1);

        return dX;
    }

    // Helper function to calculate the Sobel gradient in the Y direction
    private static float GetSobelGradientY(Bitmap heightMap, int x, int y)
    {
        float dY = 0.0f;

        dY += -1.0f * GetHeightValue(heightMap, x - 1, y - 1);
        dY += -2.0f * GetHeightValue(heightMap, x, y - 1);
        dY += -1.0f * GetHeightValue(heightMap, x + 1, y - 1);

        dY += 1.0f * GetHeightValue(heightMap, x - 1, y + 1);
        dY += 2.0f * GetHeightValue(heightMap, x, y + 1);
        dY += 1.0f * GetHeightValue(heightMap, x + 1, y + 1);

        return dY;
    }

    // Helper function to get the height value at a given pixel
    public static float GetHeightValue(Bitmap heightMap, int x, int y)
    {
        if (x < 0) x = 0;
        if (x >= heightMap.Width) x = heightMap.Width - 1;
        if (y < 0) y = 0;
        if (y >= heightMap.Height) y = heightMap.Height - 1;

        Color color = heightMap.GetPixel(x, y);
        return color.R / 255.0f; // Assuming the height is in the red channel
    }
}
