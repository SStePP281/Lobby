using Zenject;

public class FriendPrefabInstaller : MonoInstaller
{
    public FriendElementUI friendElementUIPrefab;
    public AvatarElementUI avatarElementUIPrefab;

    public override void InstallBindings()
    {
        Container.Bind<FriendElementUI>().FromInstance(friendElementUIPrefab);
        Container.Bind<AvatarElementUI>().FromInstance(avatarElementUIPrefab);
    }
}