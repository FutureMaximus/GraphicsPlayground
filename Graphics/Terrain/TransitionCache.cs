namespace GraphicsPlayground.Graphics.Terrain;

public sealed class TransitionCache
{
    private readonly ReuseCell[] _cache;

    public TransitionCache()
    {
        const int cacheSize = 0;// 2 * TransvoxelExtractor.BlockWidth * TransvoxelExtractor.BlockWidth;
        _cache = [];

        for (int i = 0; i < cacheSize; i++)
        {
            _cache[i] = new ReuseCell(12);
        }
    }

    public ReuseCell this[int x, int y]
    {
        get
        {
            return null;//_cache[x + (y & 1) * TransvoxelExtractor.BlockWidth];
        }
        set
        {
            //_cache[x + (y & 1) * TransvoxelExtractor.BlockWidth] = value;
        }
    }
}
