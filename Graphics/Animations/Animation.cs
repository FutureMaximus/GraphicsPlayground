﻿using Assimp;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Animations;

public struct AnimationNodeInfo()
{
    public Matrix4 Transformation = Matrix4.Identity;
    public string? Name = null;
    public int ChildrenCount = 0;
    public List<AnimationNodeInfo> Children = [];
}

/// <summary>A skeletal animation that can be applied to a skeletal mesh.</summary>
public sealed class Animation
{
    public string Name;
    public float Duration = 0;
    public float TicksPerSecond = 0;
    public List<Bone> Bones = [];
    public Dictionary<string, BoneInfo> BoneInfoMap = [];
    public AnimationNodeInfo RootNode;

    public Animation(in Assimp.Animation animationToLoad, in Scene scene, Dictionary<string, BoneInfo> meshBoneInfo, int boneInfoCount)
    {
        Name = animationToLoad.Name;
        Duration = (float)animationToLoad.DurationInTicks;
        TicksPerSecond = (float)animationToLoad.TicksPerSecond;
        ReadMissingBones(animationToLoad, meshBoneInfo, boneInfoCount);
        AnimationNodeInfo root = new();
        ReadHierarchyData(ref root, scene.RootNode);
        RootNode = root;
    }

    /// <summary>Reads the missing bones of the animation.</summary>
    public void ReadMissingBones(in Assimp.Animation animationToRead, Dictionary<string, BoneInfo> meshBoneInfo, int boneInfoCount)
    { 
        int size = animationToRead.NodeAnimationChannelCount;
        for (int i = 0; i < size; i++)
        {
            NodeAnimationChannel channel = animationToRead.NodeAnimationChannels[i];
            string boneName = channel.NodeName;
            if (!meshBoneInfo.TryGetValue(boneName, out BoneInfo value))
            {
                value = new BoneInfo() { ID = boneInfoCount };
                meshBoneInfo[boneName] = value;
                boneInfoCount++;
            }
            Bones.Add(new Bone(boneName, value.ID, channel));
        }
    }

    /// <summary>Reads the hierarchy data of the animation.</summary>
    public static void ReadHierarchyData(ref AnimationNodeInfo destination, in Node source)
    {
        destination.Name = source.Name;
        Matrix4x4 m = source.Transform;
        destination.Transformation = new Matrix4(m.A1, m.A2, m.A3, m.A4, m.B1, m.B2, m.B3, m.B4, m.C1, m.C2, m.C3, m.C4, m.D1, m.D2, m.D3, m.D4);
        destination.ChildrenCount = source.ChildCount;
        foreach (Node child in source.Children)
        {
            AnimationNodeInfo childNode = new();
            ReadHierarchyData(ref childNode, child);
            if (childNode.Name == null)
            {
                continue;
            }
            destination.Children.Add(childNode);
        }
    }

    /// <summary>Gets the bone by name.</summary>
    public Bone? GetBoneByName(in string name)
    {
        foreach (Bone bone in Bones)
        {
            if (bone.Name == name)
            {
                return bone;
            }
        }
        return null;
    }
}
