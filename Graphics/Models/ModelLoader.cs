using Assimp;
using OpenTK.Mathematics;
using GraphicsPlayground.Graphics.Shaders.Data;
using GraphicsPlayground.Graphics.Textures;
using GraphicsPlayground.Util;
using GraphicsPlayground.Graphics.Models.Mesh;
using GraphicsPlayground.Graphics.Animations;

namespace GraphicsPlayground.Graphics.Models;

/// <summary>
/// Processes PBR models and stores them in a list.
/// When you get a model from the entry it is removed from the list.
/// <para>
/// For arm textures the file name must contain arm, rough, metal, or ao in it otherwise it will not be loaded.
/// If there is no arm texture then roughness, metallic and ambient occlusion textures will be loaded instead.
/// </para>
/// </summary>
public static class ModelLoader
{
    private static readonly List<Model> _processedModels = [];
    private static readonly List<LoadedTexture> _processedTextures = [];

    public struct ModelEntry(string name, string modelFile, string path, string texturePath)
    {
        public string Name = name;
        public string ModelFile = modelFile;
        public string Path = path;
        public string TexturePath = texturePath;
        public Model? CoreModel;
    }

    public struct LoadedTexture(string modelName, string name, string path)
    {
        public string ModelInternalName = modelName;
        public string Name = name;
        public string Path = path;

        public override readonly bool Equals(object? obj)
        {
            if (obj is not LoadedTexture texture)
            {
                return false;
            }
            return texture.Path == Path;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(ModelInternalName, Name, Path);
        }

        public static bool operator ==(LoadedTexture left, LoadedTexture right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LoadedTexture left, LoadedTexture right)
        {
            return !(left == right);
        }
    }

    /// <summary> Data for a node in the model. </summary>
    public struct AssimpNodeData
    {
        public string Name;
        public Matrix4 Transformation;
        public int ChildrenCount;
        public List<AssimpNodeData> Children;
    }

    /// <summary>
    /// Loads a model from the given path. You can specify the post processing steps as well.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="directory"></param>
    /// <param name="postProcessSteps"></param>
    /// <param name="assetStreamer"></param>
    /// <exception cref="AssimpException"></exception>
    public static void ProcessModel(ModelEntry modelEntry, AssetStreamer assetStreamer, PostProcessSteps postProcessSteps = 
        PostProcessSteps.Triangulate | 
        PostProcessSteps.SortByPrimitiveType |
        PostProcessSteps.FlipUVs |
        PostProcessSteps.CalculateTangentSpace
    )
    {
        AssimpContext importer = new();
        Scene scene;
        try
        {
            scene = importer.ImportFile(Path.Combine(modelEntry.Path, modelEntry.ModelFile), postProcessSteps);
            if (scene == null)
            {
                throw new AssimpException($"Could not get scene while loading model {modelEntry.Name}");
            }
        }
        catch (AssimpException e)
        {
            throw new AssimpException($"Failed to load model {modelEntry.Name}: {e}");
        }
        string internalName = modelEntry.Path.Split('\\').Last().Split('/').Last();
        Model model = new(internalName);
        modelEntry.CoreModel = model;
        _processedModels.Add(model);
        ProcessNode(scene.RootNode, scene, modelEntry, assetStreamer);
    }

    /// <summary>
    /// After getting a model from the entry, it is removed from the processed models list.
    /// </summary>
    /// <param name="name"></param>
    /// <returns>
    /// The model if it exists, otherwise null.
    /// </returns>
    public static Model? GetModel(string name)
    {
        for (int i = 0; i < _processedModels.Count; i++)
        {
            if (_processedModels[i].Name == name)
            {
                Model model = _processedModels[i];
                _processedModels.RemoveAt(i);
                foreach (LoadedTexture loadedTex in _processedTextures.ToList())
                {
                    if (loadedTex.ModelInternalName == model.Name)
                    {
                        _processedTextures.Remove(loadedTex);
                    }
                }
                return model;
            }
        }
        return null;
    }

    private static void ProcessNode(Node Node, Scene scene, ModelEntry modelEntry, AssetStreamer assetStreamer, ModelPart? parent = null)
    {
        if (modelEntry.CoreModel is null) return;
        ModelPart modelPart = new(Node.Name, modelEntry.CoreModel);
        if (parent is not null)
        {
            modelPart.Parent = parent;
        }

        Matrix4x4 transform = Node.Transform;
        Matrix4 modelTransform = new(
                  transform.A1, transform.A2, transform.A3, transform.A4,
                  transform.B1, transform.B2, transform.B3, transform.B4,
                  transform.C1, transform.C2, transform.C3, transform.C4,
                  transform.D1, transform.D2, transform.D3, transform.D4);
        modelTransform.Transpose();
        modelPart.LocalTransformation = new(modelTransform);

        modelEntry.CoreModel.Parts.Add(modelPart);

        for (int i = 0; i < Node.MeshCount; i++)
        {
            Assimp.Mesh assimpMesh = scene.Meshes[Node.MeshIndices[i]];
            IMesh? modelMesh;
            if (assimpMesh.BoneCount > 0)
            {
                modelMesh = ProcessMesh(typeof(SkeletalMesh), assimpMesh, scene, modelEntry, assetStreamer, modelPart);
            }
            else
            {
                modelMesh = ProcessMesh(typeof(GenericMesh), assimpMesh, scene, modelEntry, assetStreamer, modelPart);
            }
            if (modelMesh is null)
            {
                continue;
            }
            modelPart.Meshes.Add(modelMesh);
        }

        for (int i = 0; i < Node.ChildCount; i++)
        {
            ProcessNode(Node.Children[i], scene, modelEntry, assetStreamer, modelPart);
        }
    }

    private static void ExtractBonesForSkeletalMesh(Assimp.Mesh assimpMesh, ref SkeletalMesh skeletalMesh)
    {
        for (int boneIndex = 0; boneIndex < assimpMesh.BoneCount; boneIndex++)
        {
            int boneID;
            string boneName = assimpMesh.Bones[boneIndex].Name;
            if (!skeletalMesh.BoneInfoMap.TryGetValue(boneName, out BoneInfo boneInfo))
            {
                Matrix4x4 offsetMat = assimpMesh.Bones[boneIndex].OffsetMatrix;
                // TODO: Is this properly implemented?
                Matrix4 offset = new(offsetMat.A1, offsetMat.A2, offsetMat.A3, offsetMat.A4,
                    offsetMat.B1, offsetMat.B2, offsetMat.B3, offsetMat.B4,
                    offsetMat.C1, offsetMat.C2, offsetMat.C3, offsetMat.C4,
                    offsetMat.D1, offsetMat.D2, offsetMat.D3, offsetMat.D4);
                BoneInfo newBoneInfo = new(skeletalMesh.BoneCounter, offset);
                skeletalMesh.BoneInfoMap.Add(boneName, newBoneInfo);
                boneID = skeletalMesh.BoneCounter;
                skeletalMesh.BoneCounter++;
            }
            else
            {
                boneID = boneInfo.ID;
            }

            if (boneID == -1)
            {
                throw new AssimpException($"Bone ID is -1 for bone {boneName} in skeletal mesh {skeletalMesh.Name}");
            }

            List<VertexWeight> weights = assimpMesh.Bones[boneIndex].VertexWeights;
            int weightCount = assimpMesh.Bones[boneIndex].VertexWeightCount;

            for (int i = 0; i < weightCount; i++)
            {
                int vertexID = weights[i].VertexID;
                float weight = weights[i].Weight;

                if (vertexID >= skeletalMesh.Vertices.Count)
                {
                    throw new AssimpException($"Vertex ID {vertexID} is greater than the vertex count of " +
                        $"{skeletalMesh.Vertices.Count} in skeletal mesh {skeletalMesh.Name}");
                }
                if (i >= GraphicsUtil.MaxBoneInfluence)
                {
                    throw new AssimpException($"Bone influence count is greater than the " +
                        $"maximum bone influence count of {GraphicsUtil.MaxBoneInfluence} in skeletal mesh {skeletalMesh.Name}");
                }

                for (int j = 0; j < GraphicsUtil.MaxBoneInfluence; j++)
                {
                    skeletalMesh.BoneIDs[vertexID * GraphicsUtil.MaxBoneInfluence + j] = boneID;
                    skeletalMesh.Weights[vertexID * GraphicsUtil.MaxBoneInfluence + j] = weight;
                }
            }
        }
    }

    private static IMesh? ProcessMesh(Type meshType, Assimp.Mesh mesh, Scene scene, ModelEntry modelEntry, AssetStreamer assetStreamer, ModelPart modelPart)
    {
        List<Vector3> positions = [];
        List<Vector3> normals = [];
        List<Vector2> textureCoords = [];
        List<Vector3> tangents = [];
        List<uint> indices = [];

        for (int i = 0; i < mesh.VertexCount; i++)
        {
            if (!mesh.HasNormals)
            {
                throw new AssimpException($"Mesh {mesh.Name} in model {modelEntry.Name} does not have normals.");
            }
            if (!mesh.HasTextureCoords(0))
            {
                textureCoords.Add(Vector2.Zero);
            }
            else
            {
                textureCoords.Add(new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y));
            }

            positions.Add(new Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z));
            normals.Add(new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z));
            tangents.Add(new Vector3(mesh.Tangents[i].X, mesh.Tangents[i].Y, mesh.Tangents[i].Z));
        }

        for (int i = 0; i < mesh.FaceCount; i++)
        {
            Face face = mesh.Faces[i];
            for (int j = 0; j < face.IndexCount; j++)
            {
                indices.Add((uint)face.Indices[j]);
            }
        }

        if (mesh.MaterialIndex < 0)
        {
            DebugLogger.Log($"Mesh {mesh.Name} in model {modelEntry.Name} does not have a material.");
            return null;
        }
        Material material = scene.Materials[mesh.MaterialIndex];

        // 1. Albedo maps
        List<Texture2D> albedoMaps = LoadMaterialTextures(material, TextureType.Diffuse, modelEntry, assetStreamer, mesh);

        // 2. Normal maps
        List<Texture2D> normalMaps = LoadMaterialTextures(material, TextureType.Normals, modelEntry, assetStreamer, mesh);

        // 3. ARM maps (Ambient Occlusion, Roughness, Metallic)
        List<Texture2D> armMaps = LoadMaterialTextures(material, TextureType.Unknown, modelEntry, assetStreamer, mesh);

        // 4. Height maps (Optional)
        List<Texture2D> heightMaps = LoadMaterialTextures(material, TextureType.Height, modelEntry, assetStreamer, mesh);

        Texture2D albedoTexture;
        try
        {
            albedoTexture = albedoMaps[0];
        } catch (ArgumentOutOfRangeException)
        {
            throw new AssimpException($"Mesh {mesh.Name} in model {modelEntry.Name} does not have an albedo texture.");
        }

        Texture2D normalTexture;
        try
        {
            normalTexture = normalMaps[0];
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new AssimpException($"Mesh {mesh.Name} in model {modelEntry.Name} does not have a normal texture.");
        }

        Texture2D armTexture;
        try
        {
            armTexture = armMaps[0];
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new AssimpException($"Mesh {mesh.Name} in model {modelEntry.Name} does not have an ARM texture.");
        }

        Texture2D? heightMap = heightMaps.Count > 0 ? heightMaps[0] : null;

        PBRMaterialData meshMaterial = new(albedoTexture, normalTexture, armTexture);
        if (heightMap is not null)
        {
            meshMaterial.HeightTexture = heightMap;
        }
        GenericMeshShaderData meshShaderData = new(meshMaterial); 

        GenericMesh modelMesh = new(mesh.Name, modelPart)
        {
            ShaderData = meshShaderData,
            Vertices = positions,
            TextureCoords = textureCoords,
            Normals = normals,
            Tangents = tangents,
            Indices = indices
        };

        if (meshType == typeof(SkeletalMesh))
        {
            List<int> boneIDs = [];
            List<float> weights = [];

            for (int i = 0; i < positions.Count; i++)
            {
                for (int j = 0; j < GraphicsUtil.MaxBoneInfluence; j++)
                {
                    boneIDs.Add(-1);
                    weights.Add(0.0f);
                }
            }

            SkeletalMesh skeletalMesh = new(mesh.Name, modelPart)
            {
                ShaderData = meshShaderData,
                Vertices = positions,
                TextureCoords = textureCoords,
                Normals = normals,
                Tangents = tangents,
                Indices = indices,
                BoneIDs = boneIDs,
                Weights = weights
            };
            ExtractBonesForSkeletalMesh(mesh, ref skeletalMesh);
            return skeletalMesh;
        }

        return modelMesh;
    }

    private static List<Texture2D> LoadMaterialTextures(Material mat, TextureType type, ModelEntry modelEntry, AssetStreamer assetStreamer, Assimp.Mesh mesh)
    {
        List<Texture2D> loadedTextures = [];
        for (int i = 0; i < mat.GetMaterialTextureCount(type); i++)
        {
            mat.GetMaterialTexture(type, i, out TextureSlot tex);
            string filePath = tex.FilePath;
            if (filePath == string.Empty || filePath == null) continue;

            if (type == TextureType.Unknown
                && !filePath.Contains("arm") 
                && !filePath.Contains("rough") 
                && !filePath.Contains("metal")
                && !filePath.Contains("ao"))
            {
                throw new AssimpException($"Mesh {mesh.Name} in model {modelEntry.Name} does not have an ARM texture.");
            }

            string internalModelName = modelEntry.CoreModel?.Name ?? throw new Exception("Model entry core model is null.");
            string texName = $"{internalModelName}_{Path.GetFileNameWithoutExtension(filePath)}";
            LoadedTexture loadedTexture = new(internalModelName, texName, tex.FilePath);

            if (_processedTextures.Contains(loadedTexture))
            {
                TextureEntries.GetTexture(loadedTexture.Name, out Texture2D? texture);
                loadedTextures.Add(texture);
                break;
            }
            _processedTextures.Add(loadedTexture);

            string texturePath = Path.Combine(modelEntry.Path, filePath);
            Texture2D materialTexture = new(texName);
            TextureEntries.AddTexture(materialTexture);
            TextureHelper.LoadTextureFromAssetStreamer(materialTexture, texturePath, assetStreamer);
            loadedTextures.Add(materialTexture);
        }
        return loadedTextures;
    }
}
