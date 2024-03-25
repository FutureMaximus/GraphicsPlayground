using Assimp;

namespace GraphicsPlayground.Graphics.Animations;

public static class AnimationHandler
{
    public static readonly Dictionary<string, Animation> Animations = [];

    public static void AddAnimation(Assimp.Animation animationToLoad, in Scene scene, Dictionary<string, BoneInfo> boneInfo, int boneInfoCount)
    {
        if (Animations.ContainsKey(animationToLoad.Name))
        {
            throw new ArgumentException($"Animation with name {animationToLoad.Name} already exists.");
        }

        Animation animation = new(animationToLoad, scene, boneInfo, boneInfoCount);
        Animations.Add(animation.Name, animation);
    }

    public static void RemoveAnimation(string name)
    {
        if (!Animations.ContainsKey(name))
        {
            throw new ArgumentException($"Animation with name {name} does not exist.");
        }

        Animations.Remove(name);
    }
}
