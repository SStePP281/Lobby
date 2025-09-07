using ModestTree;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "FriendSOInstaller", menuName = "Installers/FriendSOInstaller")]
public class FriendSOInstaller : ScriptableObjectInstaller<FriendSOInstaller>
{
    [SerializeField] private FriendAvatarListSO avatarList;

    public override void InstallBindings()
    {
        Assert.IsNotNull(avatarList, nameof(avatarList));

        Container.Bind<FriendAvatarListSO>().FromInstance(avatarList);
    }
}