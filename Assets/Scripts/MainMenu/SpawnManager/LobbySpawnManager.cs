using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class LobbySpawnManager : MonoBehaviour
{
    [Inject] private ICharacterFactory characterFactory;

    [SerializeField] private List<Transform> spawnPointList;

    private CharacterData currentCharacterData;
    private Transform currentCharacter;

    private void Awake()
    {
        HashDataManager.Instance.OnUserDataChanged += HashDataManager_OnUserDataChanged;
    }

    private void UpdateVisual()
    {
        CharacterData characterData = HashDataManager.Instance.GetMyCharacterData();

        if (currentCharacter != null && characterData.CharacterIndex == currentCharacterData.CharacterIndex) { return; }

        currentCharacterData = characterData;

        if (currentCharacter != null)
        {
            Destroy(currentCharacter.gameObject);
        }

        currentCharacter = characterFactory.CreateCharacter(currentCharacterData.CharacterIndex, spawnPointList[0]);
    }

    private void HashDataManager_OnUserDataChanged() => UpdateVisual();

    private void OnDestroy()
    {
        if (HashDataManager.Instance != null)
        {
            HashDataManager.Instance.OnFriendDataChanged -= HashDataManager_OnUserDataChanged;
        }
    }
}
