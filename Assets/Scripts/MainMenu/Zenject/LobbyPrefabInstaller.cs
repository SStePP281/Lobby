using Zenject;

public class LobbyPrefabInstaller : MonoInstaller
{
    public FriendElementUI friendElementUIPrefab;
    public AvatarElementUI avatarElementUIPrefab;
    public CharacterElementUI characterElementUIPrefab;

    public override void InstallBindings()
    {
        Container.Bind<FriendElementUI>().FromInstance(friendElementUIPrefab);
        Container.Bind<AvatarElementUI>().FromInstance(avatarElementUIPrefab);
        Container.Bind<CharacterElementUI>().FromInstance(characterElementUIPrefab);
    }
}