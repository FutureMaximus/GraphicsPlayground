using System.Collections.Concurrent;
using GraphicsPlayground.Graphics.Render;
using System.Drawing;

namespace GraphicsPlayground.Util;

public interface IAssetHolder : IDisposable
{
    public string Name { get; }
}

/// <summary> Used to load heavy assets like textures without blocking the main thread. </summary>
public class AssetStreamer(Engine engine)
{
    /// <summary> Adds tasks to the engine's task queue to load the given assets on the main thread. </summary>
    public Engine Engine { get; } = engine;
    /// <summary>
    /// The queue of assets to load.
    /// </summary>
    public ConcurrentQueue<IAssetHolder> AssetsToLoad { get; } = new();

    private bool _assetsLoading = false;

    /// <summary>
    /// Loads an asset from the given path and executes the given action when it is loaded.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="onLoaded"></param>
    /// <param name="assetData"></param>
    /// <exception cref="FileNotFoundException"></exception>
    public void LoadAsset(IAssetHolder assetHolder)
    {
        AssetsToLoad.Enqueue(assetHolder);
        if (!_assetsLoading)
        {
            RunAssetLoader();
        }
    }

    private void RunAssetLoader()
    {
        _assetsLoading = true;
        Task.Run(async () =>
        {
            FileStreamOptions fileStreamOptions = new()
            {
                Mode = FileMode.Open,
                Access = FileAccess.Read,
                Share = FileShare.Read,
                BufferSize = 4096,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
            };

            while (AssetsToLoad.TryDequeue(out IAssetHolder? assetHolder))
            {
                if (assetHolder is StreamingAsset asset)
                {
                    using FileStream fileStream = new(asset.Path, fileStreamOptions);
                    byte[] data = new byte[fileStream.Length];
                    await fileStream.ReadAsync(data);
                    asset.Data = data;
                    asset.ActionAfterDataLoaded?.Invoke(asset.Data, asset);
                    asset.Loaded = true;
                    fileStream.Close();
                    Engine.StreamedAssets.Push(asset);
                }
                else if (assetHolder is StreamingAssetPackage package)
                {
                    byte[][] data = new byte[package.Paths.Length][];
                    for (int i = 0; i < package.Paths.Length; i++)
                    {
                        using FileStream fileStream = new(package.Paths[i], fileStreamOptions);
                        byte[] fileData = new byte[fileStream.Length];
                        await fileStream.ReadAsync(fileData);
                        data[i] = fileData;
                        fileStream.Close();
                    }
                    package.Data = data;
                    package.ActionAfterDataLoaded?.Invoke(package.Data, package);
                    package.Loaded = true;
                    Engine.StreamedAssets.Push(package);
                }
                DebugLogger.Log($"<{KnownColor.Blue}>Loaded asset <white>{assetHolder.Name}");
            }
            _assetsLoading = false;
        });
    }

    public class StreamingAsset(string name, string path, Action<byte[], object?> afterLoaded, object? assetData = null) : IAssetHolder
    {
        public string Name = name;
        public string Path = path;
        /// <summary>Specify an optional action to execute after the data is loaded while the asset is on a separate thread.</summary>
        public Action<byte[], StreamingAsset>? ActionAfterDataLoaded;
        /// <summary>Specify the action that occurs after the asset is loaded this will run on the main thread.</summary>
        public Action<byte[], object?> AfterLoadedExecute = afterLoaded;
        public bool Loaded;
        public byte[]? Data;
        public object? AssetObjectData = assetData;

        string IAssetHolder.Name
        {
            get => Name;
        }

        public void Dispose()
        {
            Data = null;
            AssetObjectData = null;
            GC.SuppressFinalize(this);
        }
    }


    public class StreamingAssetPackage(string name, string[] paths, Action<byte[][], object?> afterLoaded, object? assetData) : IAssetHolder
    {
        public string Name = name;
        public string[] Paths = paths;
        /// <summary>
        /// Specify an optional action to execute after the data is loaded while the asset is on a separate thread.
        /// </summary>
        public Action<byte[][], StreamingAssetPackage>? ActionAfterDataLoaded;
        /// <summary>
        /// Specify the action that occurs after the asset is loaded this will run on the main thread.
        /// </summary>
        public Action<byte[][], object?> AfterLoadedExecute = afterLoaded;
        public bool Loaded;
        public byte[][]? Data;
        public object? AssetObjectData = assetData;

        string IAssetHolder.Name
        {
            get => Name;
        }

        public void Dispose()
        {
            Data = null;
            AssetObjectData = null;
            GC.SuppressFinalize(this);
        }
    }
}