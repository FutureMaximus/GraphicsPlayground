namespace GraphicsPlayground.Graphics.Animations;

/// <summary> A skeleton that contains a hierarchy of bones that can be animated. </summary>
public class Skeleton
{
    public Dictionary<int, Bone> Bones { get; set; } = [];
    public Bone RootBone { get; set; }
    public int BoneCounter { get; set; } = 0;
    public int BoneCount => Bones.Count;

    public Skeleton(Bone rootBone)
    {
        RootBone = rootBone;
        Bones.Add(RootBone.Name.GetHashCode(), rootBone);
    }

    public void AddBone(Bone bone)
    {
        Bones.Add(bone.Name.GetHashCode(), bone);
    }

    public Bone GetBone(string name)
    {
        return Bones[name.GetHashCode()];
    }

    public Bone GetBone(int index)
    {
        return Bones.Values.ElementAt(index);
    }
}
