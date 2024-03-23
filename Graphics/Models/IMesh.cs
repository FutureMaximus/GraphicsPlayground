using GraphicsPlayground.Graphics.Shaders.Data;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Models;

///<summary>Represents a renderable mesh.</summary>
public interface IMesh : IDisposable
{
    ///<summary>The model part that owns this mesh.</summary>
    ModelPart ParentPart { get; set; }

    ///<summary>The vertices of the mesh.</summary>
    List<Vector3> Vertices { get; set; }

    ///<summary>The indices of the mesh.</summary>
    List<uint> Indices { get; set; }

    ///<summary>The name of the mesh.</summary>
    string Name { get; set; }

    ///<summary>The level of detail of the mesh.</summary>
    LODInfo LOD { get; set; }

    ///<summary>The unique identifier of the mesh this should be the same across several LODs of the same mesh.</summary>
    Guid ID { get; set; }

    ///<summary>The buffer usage hint of the mesh.</summary>
    BufferUsageHint MeshUsageHint { get; }

    ///<summary>Loads the mesh data into OpenGL.</summary>
    void Load();

    ///<summary>Renders the mesh.</summary>
    void Render();

    ///<summary>ShaderProgram data for this mesh.</summary>
    IShaderData ShaderData { get; set; }
}
