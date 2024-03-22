using GraphicsPlayground.Graphics.Textures;
using GraphicsPlayground.Util;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Render.RenderPasses.SubPasses;

public class ScreenFBO(Shader shader, Vector2i windowSize) : IRenderPass
{
    public Shader ScreenShader = shader;
    public Vector2i WindowSize = windowSize;
    public int FramebufferObject;
    public Texture2D? ScreenTexture;
    public int RenderBufferObject;

    public int ScreenQuadVAO;
    public int ScreenQuadVBO;

    public bool IsEnabled { get; set; }

    public void Load()
    {
        ScreenShader.Use();
        ScreenQuadVAO = GL.GenVertexArray();
        ScreenQuadVBO = GL.GenBuffer();
        GL.BindVertexArray(ScreenQuadVAO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, ScreenQuadVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, _screenQuadVertices.Length * sizeof(float), _screenQuadVertices, BufferUsageHint.StaticDraw);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
        GraphicsUtil.CheckError("ScreenFBO Load");

        ScreenShader.Use();
        GraphicsUtil.CheckError("ScreenFBO Shader Use");

        FramebufferObject = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FramebufferObject);

        ScreenTexture = new Texture2D("ScreenTexture", GL.GenTexture());
        TextureEntries.AddTexture(ScreenTexture);
        GL.BindTexture(TextureTarget.Texture2D, ScreenTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, WindowSize.X, WindowSize.Y, 0, PixelFormat.Rgb, PixelType.UnsignedByte, nint.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, ScreenTexture, 0);
        GraphicsUtil.CheckError("ScreenFBO FramebufferTexture2D");

        RenderBufferObject = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RenderBufferObject);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, WindowSize.X, WindowSize.Y);
        GraphicsUtil.CheckError("ScreenFBO RenderbufferStorage");

        GL.FramebufferRenderbuffer(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.DepthStencilAttachment,
            RenderbufferTarget.Renderbuffer,
            RenderBufferObject);
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
        GraphicsUtil.CheckError("ScreenFBO FramebufferRenderbuffer");

        if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
        {
            throw new Exception("Screen framebuffer is not complete.");
        }

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private readonly float[] _screenQuadVertices =
    [
        // Positions   // TexCoords
        -1.0f,  1.0f,  0.0f, 1.0f,
        -1.0f, -1.0f,  0.0f, 0.0f,
         1.0f, -1.0f,  1.0f, 0.0f,

        -1.0f,  1.0f,  0.0f, 1.0f,
         1.0f, -1.0f,  1.0f, 0.0f,
         1.0f,  1.0f,  1.0f, 1.0f
    ];

    public void Bind() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, FramebufferObject);

    public void Resize(Vector2i windowSize)
    {
        if (ScreenTexture is null) return;
        WindowSize = windowSize;
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FramebufferObject);
        GL.BindTexture(TextureTarget.Texture2D, ScreenTexture);
        GL.TexImage2D(TextureTarget.Texture2D,
            0,
            PixelInternalFormat.Rgb,
            WindowSize.X,
            WindowSize.Y,
            0,
            PixelFormat.Rgb, 
            PixelType.UnsignedByte, 
            nint.Zero);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, ScreenTexture, 0);
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RenderBufferObject);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, WindowSize.X, WindowSize.Y);
        GL.FramebufferRenderbuffer(
                       FramebufferTarget.Framebuffer,
                       FramebufferAttachment.DepthStencilAttachment,
                       RenderbufferTarget.Renderbuffer,
                       RenderBufferObject);
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
    }

    public void Render()
    {
        if (ScreenTexture is null) return;
        GL.BindVertexArray(ScreenQuadVAO);
        ScreenTexture.Use(TextureUnit.Texture0);
        ScreenShader.Use();
        ScreenShader.SetInt("screenTexture", 0);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }

    public void Dispose()
    {
        GL.DeleteBuffer(ScreenQuadVBO);
        GL.DeleteVertexArray(ScreenQuadVAO);
        GL.DeleteFramebuffer(FramebufferObject);
        ScreenTexture?.Dispose();
        GL.DeleteRenderbuffer(RenderBufferObject);
        GC.SuppressFinalize(this);
    }
}