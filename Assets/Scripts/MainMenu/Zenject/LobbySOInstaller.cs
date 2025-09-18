using ModestTree;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "LobbySOInstaller", menuName = "Installers/LobbySOInstaller")]
public class LobbySOInstaller : ScriptableObjectInstaller<LobbySOInstaller>
{
    public LobbyAvatarListSO avatarList;
    public LobbyCharacterSOList lobbyCharacterSOList;

    public override void InstallBindings()
    {
        Assert.IsNotNull(avatarList, nameof(avatarList));
        Assert.IsNotNull(lobbyCharacterSOList, nameof(lobbyCharacterSOList));

        Container.Bind<LobbyAvatarListSO>().FromInstance(avatarList);
        Container.Bind<LobbyCharacterSOList>().FromInstance(lobbyCharacterSOList);
    }
}