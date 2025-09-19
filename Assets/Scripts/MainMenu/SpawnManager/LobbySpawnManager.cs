using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class LobbySpawnManager : MonoBehaviour
{
    [Inject] private ICharacterFactory characterFactory;

    [SerializeField] private List<Transform> spawnPointList;

    private Transform currentCharacter;

    private void Awake()
    {
        HashDataManager.Instance.OnPlayerDataChanged += HashDataManager_OnPlayerDataChanged;
    }

    private void UpdateVisual()
    {
        CharacterData characterData = HashDataManager.Instance.GetMyCharacterData();

        if (currentCharacter != null)
        {
            Destroy(currentCharacter.gameObject);
        }

        currentCharacter = characterFactory.CreateCharacter(characterData.CharacterIndex, spawnPointList[0]);
    }

    private void HashDataManager_OnPlayerDataChanged() => UpdateVisual();

    private void OnDestroy()
    {
        if (HashDataManager.Instance != null)
        {
            HashDataManager.Instance.OnPlayerDataChanged -= HashDataManager_OnPlayerDataChanged;
        }
    }
}
