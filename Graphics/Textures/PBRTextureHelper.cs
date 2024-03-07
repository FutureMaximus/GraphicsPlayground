using GraphicsPlayground.Util;

namespace GraphicsPlayground.Graphics.Textures;

public static class PBRTextureHelper
{
    /// <summary>
    /// <para>
    /// Loads PBR textures from a given name and directory path.
    /// The texture file names must contain the following:
    /// </para>
    /// <para>
    /// albedo,
    /// normal,
    /// metallic,
    /// roughness,
    /// ao,
    /// or arm.
    /// </para>
    /// <para>
    /// If there is no arm texture, ao, metallic, or roughness texture will be used instead as an arm texture.
    /// </para>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="path"></param>
    /// <param name="assetStreamer"></param>
    /// <param name="ambientOcclusion"></param>
    public static void LoadPBRTexturesFromName(string name, string path, AssetStreamer assetStreamer)
    {
        string[] files = Directory.GetFiles(path);
        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            string fileExtension = Path.GetExtension(file);
            if (fileName.Contains("albedo") || fileName.Contains("diff"))
            {
                Texture2D albedoTex = new($"{name}_albedo");
                TextureEntries.AddTexture(albedoTex);
                TextureHelper.LoadTextureFromAssetStreamer(albedoTex, $"{path}\\{fileName}{fileExtension}", assetStreamer);
            }
            else if (fileName.Contains("normal") || fileName.Contains("nor_gl"))
            {
                Texture2D normalTex = new($"{name}_normal");
                TextureEntries.AddTexture(normalTex);
                TextureHelper.LoadTextureFromAssetStreamer(normalTex, $"{path}\\{fileName}{fileExtension}", assetStreamer);
            }
            else if (fileName.Contains("arm") || fileName.Contains("rough") || fileName.Contains("metal") || fileName.Contains("ao"))
            {
                Texture2D armTex = new($"{name}_arm");
                TextureEntries.AddTexture(armTex);
                TextureHelper.LoadTextureFromAssetStreamer(armTex, $"{path}\\{fileName}{fileExtension}", assetStreamer);
            }
            else if (fileName.Contains("disp") || fileName.Contains("height"))
            {
                Texture2D heightTex = new($"{name}_height");
                TextureEntries.AddTexture(heightTex);
                TextureHelper.LoadTextureFromAssetStreamer(heightTex, $"{path}\\{fileName}{fileExtension}", assetStreamer);
            }
        }
    }
}
