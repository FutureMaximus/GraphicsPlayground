using OpenTK.Mathematics;
using GraphicsPlayground.Graphics.Models.Generic;
using GraphicsPlayground.Graphics.Shader.Data;
using GraphicsPlayground.Graphics.Shaders.Data;

namespace GraphicsPlayground.Graphics.Models;

public static class GeometryHelper
{
    /// <summary>
    /// Calculates the tangent vectors from mesh data. If tangents cannot be calculated
    /// then the shader will calculate them using the normal map at the cost of performance.
    /// </summary>
    public static List<Vector3>? CalculateTangents(List<Vector3> positions, List<Vector2> textureCoordinates, List<Vector3> normals, List<uint> indices)
    {
        List<Vector3> tangents = [];

        if (positions.Count != normals.Count || positions.Count != textureCoordinates.Count || positions.Count % 3 != 0)
            return null;

        for (int i = 0; i < indices.Count; i += 3)
        {
            Vector3 v0 = positions[(int)indices[i]];
            Vector3 v1 = positions[(int)indices[i + 1]];
            Vector3 v2 = positions[(int)indices[i + 2]];

            Vector2 uv0 = textureCoordinates[(int)indices[i]];
            Vector2 uv1 = textureCoordinates[(int)indices[i + 1]];
            Vector2 uv2 = textureCoordinates[(int)indices[i + 2]];

            Vector3 deltaPos1 = v1 - v0;
            Vector3 deltaPos2 = v2 - v0;

            Vector2 deltaUV1 = uv1 - uv0;
            Vector2 deltaUV2 = uv2 - uv0;

            float r = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV1.Y * deltaUV2.X);
            Vector3 tangent = (deltaPos1 * deltaUV2.Y - deltaPos2 * deltaUV1.Y) * r;
            Vector3 normal = normals[(int)indices[i]];
            tangent = (tangent - normal * Vector3.Dot(normal, tangent)).Normalized();

            tangents.Add(tangent);
            tangents.Add(tangent);
            tangents.Add(tangent);
        }

        if (tangents.Count == 0)
            return null;

        return tangents;
    }

    /// <summary>
    /// Creates a list of VertexData from a list of positions, normals, and texture coordinates.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static List<GenericVertexData> GetVertexDatas(
        List<Vector3> positions, List<Vector3> normals, List<Vector2> textureCoordinates)
    {
        if (positions.Count != normals.Count || positions.Count != textureCoordinates.Count)
            throw new ArgumentException("Position, normal, and texture coordinate lists must have the same length.");

        List<GenericVertexData> vertexData = [];

        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 position = positions[i];
            Vector3 normal = normals[i];
            Vector2 textureCoordinate = textureCoordinates[i];

            vertexData.Add(new GenericVertexData(position, normal, textureCoordinate));
        }

        return vertexData;
    }

    /// <summary>
    /// Creates a list of VertexData from a list of positions, normals, and texture coordinates.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static List<SkeletalVertexData> GetSkeletalVertexDatas(
        List<Vector3> positions, List<Vector3> normals, List<Vector2> textureCoordinates, List<int> boneIds, List<float> weights)
    {
        if (positions.Count != normals.Count || positions.Count != textureCoordinates.Count)
            throw new ArgumentException("Position, normal, and texture coordinate lists must have the same length.");

        List<SkeletalVertexData> vertexData = [];

        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 position = positions[i];
            Vector3 normal = normals[i];
            Vector2 textureCoordinate = textureCoordinates[i];
            BoneWeight boneWeight1 = new(boneIds[i], weights[i]);
            BoneWeight boneWeight2 = new(boneIds[i + 1], weights[i + 1]);
            BoneWeight boneWeight3 = new(boneIds[i + 2], weights[i + 2]);
            BoneWeight boneWeight4 = new(boneIds[i + 3], weights[i + 3]);

            SkeletalVertexData data = new(position, normal, textureCoordinate, boneWeight1, boneWeight2, boneWeight3, boneWeight4);

            vertexData.Add(data);
        }

        return vertexData;
    }


    /// <summary>
    /// Creates a list of VertexData from a list of positions, normals, and texture coordinates and calculates the tangent and bitangent vectors.
    /// </summary>
    /// <param name="positions"></param>
    /// <param name="normals"></param>
    /// <param name="textureCoordinates"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static List<GenericVertexData> GetVertexDatas(List<Vector3> positions, List<Vector3> normals, List<Vector2> textureCoordinates, uint[] indices)
    {
        if (positions.Count != normals.Count || positions.Count != textureCoordinates.Count)
            throw new ArgumentException("Position, normal, and texture coordinate lists must have the same length.");

        List<GenericVertexData> vertexData = [];

        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 position = positions[i];
            Vector3 normal = normals[i];
            Vector2 textureCoordinate = textureCoordinates[i];

            vertexData.Add(new GenericVertexData(position, normal, textureCoordinate));
        }

        return vertexData;
    }

    /// <summary> Creates a list of Vector3 from an array of floats. </summary>
    public static List<Vector3> ListOfVector3FromFloatArray(float[] vectors)
    {
        if (vectors.Length % 3 != 0)
            throw new ArgumentException("Float array must have a length that is a multiple of 3.");

        List<Vector3> vectorList = [];

        for (int i = 0; i < vectors.Length; i += 3)
        {
            vectorList.Add(new Vector3(vectors[i], vectors[i + 1], vectors[i + 2]));
        }

        return vectorList;
    }

    /// <summary> Creates a list of Vector2 from an array of floats. </summary>
    public static List<Vector2> ListOfVector2FromFloatArray(float[] vectors)
    {
        if (vectors.Length % 2 != 0)
            throw new ArgumentException("Float array must have a length that is a multiple of 2.");

        List<Vector2> vectorList = [];

        for (int i = 0; i < vectors.Length; i += 2)
        {
            vectorList.Add(new Vector2(vectors[i], vectors[i + 1]));
        }

        return vectorList;
    }

    /// <summary> Creates a floating point array from a list of Vector3 </summary>
    public static float[] ArrayFromVector3List(List<Vector3> vecList)
    {
        List<float> floatList = [];

        foreach (Vector3 vec in vecList)
        {
            floatList.AddRange(new float[] { vec.X, vec.Y, vec.Z });
        }

        return [.. floatList];
    }

    /// <summary> Creates a floating point array from a list of Vector2 </summary>
    public static float[] ArrayFromVector2List(List<Vector2> vecList)
    {
        List<float> floatList = [];

        foreach (Vector2 vec in vecList)
        {
            floatList.AddRange(new float[] { vec.X, vec.Y });
        }

        return [.. floatList];
    }

    /// <summary> 
    /// Creates a vertex array from a list of positions, normals, and texture coordinates.
    /// Note that the positions, normals, and texture coordinates must all have the same length.
    /// </summary>
    public static float[] InterleavedArrayPositionNormalsTexture(List<Vector3> positions, List<Vector3> normals, List<Vector2> textureCoordinates)
    {
        if (positions.Count != normals.Count || positions.Count != textureCoordinates.Count)
            throw new ArgumentException("Position, normal, and texture coordinate lists must have the same length.");

        List<float> interleavedData = [];

        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 position = positions[i];
            Vector3 normal = normals[i];
            Vector2 textureCoordinate = textureCoordinates[i];

            interleavedData.AddRange(new float[] { position.X, position.Y, position.Z, normal.X, normal.Y, normal.Z, textureCoordinate.X, textureCoordinate.Y });
        }

        return [.. interleavedData];
    }

    /// <summary> 
    /// Creates a vertex array from a list of positions, normals, and 3D texture coordinates.
    /// Note that the positions, normals, and texture coordinates must all have the same length.
    /// </summary>
    public static float[] InterleavedArrayPositionNormalsTexture3D(List<Vector3> positions, List<Vector3> normals, List<Vector3> textureCoordinates)
    {
        if (positions.Count != normals.Count || positions.Count != textureCoordinates.Count)
            throw new ArgumentException("Position, normal, and texture coordinate lists must have the same length.");

        List<float> interleavedData = [];

        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 position = positions[i];
            Vector3 normal = normals[i];
            Vector3 textureCoordinate = textureCoordinates[i];

            interleavedData.AddRange(new float[] { position.X, position.Y, position.Z, normal.X, normal.Y, normal.Z, textureCoordinate.X, textureCoordinate.Y });
        }

        return [.. interleavedData];
    }

    /// <summary> 
    /// Creates a vertex array from a list of positions and texture coordinates.
    /// Note that the positions and texture coordinates must all have the same length.
    /// </summary>
    public static float[] InterleavedArrayPositionTexture(List<Vector3> positions, List<Vector2> textureCoordinates)
    {
        if (positions.Count != textureCoordinates.Count)
            throw new ArgumentException("Position and texture coordinate lists must have the same length.");

        List<float> interleavedData = [];

        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 position = positions[i];
            Vector2 textureCoordinate = textureCoordinates[i];

            interleavedData.AddRange(new float[] { position.X, position.Y, position.Z, textureCoordinate.X, textureCoordinate.Y });
        }

        return [.. interleavedData];
    }

    /// <summary> 
    /// Creates a vertex array from a list of positions and normals.
    /// Note that the positions and normals must all have the same length.
    /// </summary>
    public static float[] InterleavedArrayPositionNormals(List<Vector3> positions, List<Vector3> normals)
    {
        if (positions.Count != normals.Count)
            throw new ArgumentException("Position and normal lists must have the same length.");

        List<float> interleavedData = [];

        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 position = positions[i];
            Vector3 normal = normals[i];

            interleavedData.AddRange(new float[] { position.X, position.Y, position.Z, normal.X, normal.Y, normal.Z });
        }

        return [.. interleavedData];
    }
}
