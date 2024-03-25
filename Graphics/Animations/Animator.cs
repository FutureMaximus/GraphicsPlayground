using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Animations;

///<summary>Responsible for updating the current animation.</summary>
public class Animator
{
    public float CurrentTime;
    public float DeltaTime;
    public Animation? CurrentAnimation;
    public Matrix4[] FinalBoneMatrices;

    public Animator(Animation animation)
    {
        CurrentTime = 0;
        DeltaTime = 0;
        CurrentAnimation = animation;
        FinalBoneMatrices = new Matrix4[100];
        for (int i = 0; i < 100; i++)
        {
            FinalBoneMatrices[i] = Matrix4.Identity;
        }
    }

    /// <summary> Updates the current animation. </summary>
    public void UpdateAnimation(float deltaTime)
    {
        DeltaTime = deltaTime;
        CurrentTime += deltaTime;
        if (CurrentAnimation != null)
        {
            CurrentTime += CurrentAnimation.TicksPerSecond * deltaTime;
            CurrentTime %= CurrentAnimation.Duration;
        }
    }

    /// <summary> Plays an animation. </summary>
    public void PlayAnimation(Animation animation)
    {
        CurrentAnimation = animation;
        CurrentTime = 0;
    }

    /// <summary> Calculates the final bone matrices for the current animation. </summary>
    public void CalculateBoneTransform(in AnimationNodeInfo nodeInfo, in Matrix4 parentTransform)
    {
        if (CurrentAnimation == null)
        {
            return;
        }
        string nodeName = nodeInfo.Name;
        Matrix4 nodeTransform = nodeInfo.Transformation;
        Bone? bone = CurrentAnimation.GetBoneByName(nodeName);
        if (bone != null)
        {
            bone.Update(CurrentTime);
            nodeTransform = bone.LocalTransform;
        }
        Matrix4 globalTransformation = nodeTransform * parentTransform;
        if (CurrentAnimation.BoneInfoMap.TryGetValue(nodeName, out BoneInfo value))
        {
            FinalBoneMatrices[value.ID] = value.Offset * globalTransformation;
        }
        foreach (AnimationNodeInfo child in nodeInfo.Children)
        {
            CalculateBoneTransform(child, globalTransformation);
        }
    }
}
