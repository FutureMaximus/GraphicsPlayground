using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Animations;

/// <summary>A bone in a skeletal animation.</summary>
public class Bone
{
    public string Name { get; }
    public int Index;
    public Matrix4 LocalTransform = Matrix4.Identity;
    public readonly List<KeyframeData<Vector3>> PositionData = [];
    public readonly List<KeyframeData<Quaternion>> RotationData = [];
    public readonly List<KeyframeData<Vector3>> ScaleData = [];
    public int NumberOfPositionKeys;
    public int NumberOfRotationKeys;
    public int NumberOfScaleKeys;

    public Bone(string name, int id, in Assimp.NodeAnimationChannel channel)
    {
        Name = name;
        Index = id;
        GetKeys(channel);
    }

    public void Update(float animationTime)
    {
        Matrix4 translation = InterpolatePosition(animationTime);
        Matrix4 rotation = InterpolateRotation(animationTime);
        Matrix4 scale = InterpolateScale(animationTime);
        LocalTransform = scale * rotation * translation;
    }

    /// <summary>
    /// Gets the current position index of the bone to interpolate
    /// based on the current animation time.
    /// </summary>
    /// <returns>
    /// The index of the current position keyframe to interpolate.
    /// </returns>
    public int GetPositionIndex(float animationTime)
    {
        for (int i = 0; i < PositionData.Count - 1; i++)
        {
            if (animationTime < PositionData[i + 1].Time)
            {
                return i;
            }
        }
        return 0;
    }

    /// <summary>
    /// Gets the current rotation index of the bone to interpolate
    /// based on the current animation time.
    /// </summary>
    /// <returns>
    /// The index of the current rotation keyframe to interpolate.
    /// </returns>
    public int GetRotationIndex(float animationTime)
    {
        for (int i = 0; i < RotationData.Count - 1; i++)
        {
            if (animationTime < RotationData[i + 1].Time)
            {
                return i;
            }
        }
        return 0;
    }

    /// <summary>
    /// Gets the current scale index of the bone to interpolate
    /// based on the current animation time.
    /// </summary>
    /// <returns>
    /// The index of the current scale keyframe to interpolate.
    /// </returns>
    public int GetScaleIndex(float animationTime)
    {
        for (int i = 0; i < ScaleData.Count - 1; i++)
        {
            if (animationTime < ScaleData[i + 1].Time)
            {
                return i;
            }
        }
        return 0;
    }

    ///<summary>Gets normalized value for lerp and slerp interpolation.</summary>
    public static double GetScaleFactor(double lastFrame, double nextFrame, double animationTime)
    {
        double totalTime = nextFrame - lastFrame;
        double currentTime = animationTime - lastFrame;
        double scaleFactor = currentTime / totalTime;
        return scaleFactor;
    }

    /// <summary>Figures out which position keyframes to interpolate and interpolates them.</summary>
    public Matrix4 InterpolatePosition(float animationTime)
    {
        if (PositionData.Count == 1)
        {
            return Matrix4.CreateTranslation(PositionData[0].Value);
        }
        int positionIndex = GetPositionIndex(animationTime);
        int nextPositionIndex = (positionIndex + 1) % PositionData.Count;
        float scaleFactor = (float)GetScaleFactor(PositionData[positionIndex].Time, PositionData[nextPositionIndex].Time, animationTime);
        Vector3 finalPosition = Vector3.Lerp(PositionData[positionIndex].Value, PositionData[nextPositionIndex].Value, scaleFactor);
        return Matrix4.CreateTranslation(finalPosition); // TODO: Transpose?
    }

    ///<summary>Figures out which rotation keyframes to interpolate and interpolates them.</summary>
    public Matrix4 InterpolateRotation(float animationTime)
    {
        if (RotationData.Count == 1)
        {
            return Matrix4.CreateFromQuaternion(RotationData[0].Value);
        }
        int rotationIndex = GetRotationIndex(animationTime);
        int nextRotationIndex = (rotationIndex + 1) % RotationData.Count;
        float scaleFactor = (float)GetScaleFactor(RotationData[rotationIndex].Time, RotationData[nextRotationIndex].Time, animationTime);
        OpenTK.Mathematics.Quaternion finalRotation = 
            OpenTK.Mathematics.Quaternion.Slerp(RotationData[rotationIndex].Value, RotationData[nextRotationIndex].Value, scaleFactor);
        return Matrix4.CreateFromQuaternion(finalRotation); // TODO: Transpose?
    }

    /// <summary>Figures out which scale keyframes to interpolate and interpolates them.</summary>
    public Matrix4 InterpolateScale(float animationTime)
    {
        if (ScaleData.Count == 1)
        {
            return Matrix4.CreateScale(ScaleData[0].Value);
        }
        int scaleIndex = GetScaleIndex(animationTime);
        int nextScaleIndex = (scaleIndex + 1) % ScaleData.Count;
        float scaleFactor = (float)GetScaleFactor(ScaleData[scaleIndex].Time, ScaleData[nextScaleIndex].Time, animationTime);
        Vector3 finalScale = Vector3.Lerp(ScaleData[scaleIndex].Value, ScaleData[nextScaleIndex].Value, scaleFactor);
        return Matrix4.CreateScale(finalScale); // TODO: Transpose?
    }

    /// <summary>Gets the keyframes from the animation channel.</summary>
    private unsafe void GetKeys(in Assimp.NodeAnimationChannel channel)
    {
        NumberOfPositionKeys = channel.PositionKeyCount;
        NumberOfRotationKeys = channel.RotationKeyCount;
        NumberOfScaleKeys = channel.ScalingKeyCount;

        for (int i = 0; i < NumberOfPositionKeys; i++)
        {
            Assimp.VectorKey key = channel.PositionKeys[i];
            PositionData.Add(new KeyframeData<Vector3>(key.Time, new Vector3(key.Value.X, key.Value.Y, key.Value.Z)));
        }
        for (int i = 0; i < NumberOfRotationKeys; i++)
        {
            Assimp.QuaternionKey key = channel.RotationKeys[i];
            RotationData.Add(new KeyframeData<OpenTK.Mathematics.Quaternion>(
                key.Time, 
                new Quaternion(key.Value.X, key.Value.Y, key.Value.Z, key.Value.W)));
        }
        for (int i = 0; i < NumberOfRotationKeys; i++)
        {
            Assimp.VectorKey key = channel.ScalingKeys[i];
            ScaleData.Add(new KeyframeData<Vector3>(key.Time, new Vector3(key.Value.X, key.Value.Y, key.Value.Z)));
        }
    }
}
