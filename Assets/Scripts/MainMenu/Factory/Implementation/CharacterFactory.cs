using UnityEngine;
using Zenject;

public class CharacterFactory : ICharacterFactory
{
    [Inject] private LobbyCharacterSOList lobbyCharacterSOList;

    public Transform CreateCharacter(int characterIndex, Transform spawnPoint = null)
    {
        return Object.Instantiate(lobbyCharacterSOList.List[characterIndex].characterPrefab, spawnPoint);
    }
}
