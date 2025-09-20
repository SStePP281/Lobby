using Zenject;

public class ServiceFactoryIntstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IFriendDataCreator>().To<FriendDataCreator>().AsSingle();
        Container.Bind<ILobbyUserDataCreator>().To<LobbyUserDataCreator>().AsSingle();
    }
}