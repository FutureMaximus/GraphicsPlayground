using OpenTK.Graphics.OpenGL4;
using GraphicsPlayground.Util;

namespace GraphicsPlayground.Graphics.Textures;

/// <summary> 
/// This contains the name, handle, width, and height
/// of a 2D texture however it does not contain the actual data you need to
/// generate the data at <see cref="TextureHelper"/> for loading textures.
/// </summary>
public class Texture2D : IDisposable
{
    public string Name { get; }
    public int Handle { get; } = 0;
    public int Width { get; set; }
    public int Height { get; set; }
    public bool Loaded { get; set; } = false;
    public bool IsDisposed { get; set; } = false;

    public Texture2D(string name, int handle)
    {
        Name = name;
        Handle = handle;
        GL.BindTexture(TextureTarget.Texture2D, Handle);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Texture, Handle, Name);
    }
    public Texture2D(string name)
    {
        Name = name;
        Handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, Handle);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Texture, Handle, Name);
    }

    public void Use(TextureUnit textureUnit)
    {
        GL.ActiveTexture(textureUnit);
        GL.BindTexture(TextureTarget.Texture2D, Handle);
        GraphicsUtil.CheckError($"Failed to bind texture {Name}");
    }

    public static implicit operator int(Texture2D texture) => texture.Handle;

    public void Dispose()
    {
        GL.DeleteTexture(Handle);
        TextureEntries.GetTexture(Name, out Texture2D? texture);
        if (texture is not null)
        {
            TextureEntries.RemoveTexture(Name);
        }
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }   
}
