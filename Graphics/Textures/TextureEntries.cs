using GraphicsPlayground.Util;

namespace GraphicsPlayground.Graphics.Textures;

/// <summary>
/// There cannot be two textures with the same name.
/// This was made so that textures can be referenced by name.
/// </summary>
public static class TextureEntries
{
    private static readonly List<Texture2D> _textures = [];

    /// <summary> Adds a reference to a texture. </summary>
    public static void AddTexture(in Texture2D texture)
    {
        foreach (Texture2D tex in _textures)
        {
            if (tex.Name == texture.Name)
            {
                DebugLogger.Log($"<red>Failed to add texture: <white>{texture.Name} <red>because a texture with that name already exists.");
                return;
            }
        }
        _textures.Add(texture);
    }

    /// <summary> Gets a texture by name and returns a default texture if not found. </summary>
    public static void GetTexture(in string name, out Texture2D texture)
    {
        for (int i = 0; i < _textures.Count; i++)
        {
            if (_textures[i].Name == name)
            {
                texture = _textures[i];
                return;
            }
        }
        GetTexture("ImageNotFound", out Texture2D? notFoundTex);
        if (notFoundTex is not null)
        {
            texture = notFoundTex;
            return;
        }
        throw new Exception($"Failed to get texture {name}.");
    }

    /// <summary> Gets a texture by name and returns a default texture if not found. </summary>
    public static Texture2D GetTexture(in string name)
    {
        for (int i = 0; i < _textures.Count; i++)
        {
            if (_textures[i].Name == name)
            {
                return _textures[i];
            }
        }
        GetTexture("ImageNotFound", out Texture2D? notFoundTex);
        if (notFoundTex is not null)
        {
            return notFoundTex;
        }
        throw new Exception($"Failed to get texture {name}.");
    }

    /// <summary> Removes a texture and disposes it. </summary>
    public static void RemoveTexture(in string name)
    {
        GetTexture(name, out Texture2D? texture);
        if (texture is null) return;
        _textures.Remove(texture);
    }

    public static void Dispose()
    {
        foreach (Texture2D texture in _textures.ToList())
        {
            if (texture.IsDisposed) continue;
            texture.Dispose();
        }
    }
}
