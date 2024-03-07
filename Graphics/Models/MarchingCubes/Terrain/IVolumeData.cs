using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Models.MarchingCubes.Terrain;

public interface IVolumeData<T>
{
    T this[int x, int y, int z] { get; set; }
    T this[Vector3i v] { get; set; }
    T this[Vector3 v] { get; set; }
    VolumeSize Size { get; }
}
