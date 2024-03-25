using Assimp;
using OpenTK.Mathematics;

namespace GraphicsPlayground.Graphics.Animations;

public struct AnimationNodeInfo(Matrix4 transform, string name, int childrenCount)
{
    public Matrix4 Transformation = transform;
    public string Name = name;
    public int ChildrenCount = childrenCount;
    public List<AnimationNodeInfo> Children = [];
}

public class Animation
{
    public string Name;
    public double Duration = 0;
    public double TicksPerSecond = 0;
    public Dictionary<string, BoneInfo> BoneInfoMap = [];
    public AnimationNodeInfo RootNode;

    public Animation(in Assimp.Animation animationToLoad, in Scene scene, Dictionary<string, BoneInfo> meshBoneInfo, int boneInfoCount)
    {
        Name = animationToLoad.Name;
        Duration = animationToLoad.DurationInTicks;
        TicksPerSecond = animationToLoad.TicksPerSecond;
        ReadMissingBones(animationToLoad, meshBoneInfo, boneInfoCount);
        AnimationNodeInfo root = new();
        ReadHierarchyData(ref root, scene.RootNode);
        RootNode = root;
    }

    public void ReadMissingBones(in Assimp.Animation animationToRead, Dictionary<string, BoneInfo> meshBoneInfo, int boneInfoCount)
    {
        int size = animationToRead.NodeAnimationChannelCount;
        for (int i = 0; i < size; i++)
        {
            NodeAnimationChannel channel = animationToRead.NodeAnimationChannels[i];
            string boneName = channel.NodeName;
        }
    }

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
            destination.Children.Add(childNode);
        }
    }
}
