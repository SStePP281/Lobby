using Zenject;

public class FriendFactoryIntstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IFriendElementFactory>().To<FriendElementFactory>().AsSingle();
        Container.Bind<IFriendDataCreator>().To<FrinedDataCreator>().AsSingle();
        Container.Bind<IAvatarElementFactory>().To<AvatarElementFactory>().AsSingle();
    }
}