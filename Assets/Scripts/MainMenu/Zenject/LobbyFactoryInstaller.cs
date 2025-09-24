using Zenject;

public class LobbyFactoryInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IAvatarElementFactory>().To<AvatarElementFactory>().AsSingle();
        Container.Bind<IFriendElementFactory>().To<FriendElementFactory>().AsSingle();
        Container.Bind<ICharacterElementFactory>().To<CharacterElementFactory>().AsSingle();
        Container.Bind<ICharacterFactory>().To<CharacterFactory>().AsSingle();
        Container.Bind<ILobbyPlayerSetupFactory>().To<LobbyPlayerSetupFactory>().AsSingle();
        Container.Bind<ILobbyElementFactory>().To<LobbyElementFactory>().AsSingle();
    }
}