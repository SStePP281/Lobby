using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class WardrobeMenuUI : MonoBehaviour
{
    [Inject] private LobbyCharacterSOList lobbyCharacterSOList;
    [Inject] private ICharacterElementFactory characterElementFactory;

    [Header("Reference")]
    [SerializeField] private WardrobeCharacterViewUI characterViewUI;
    [SerializeField] private Transform characterContainer;
    [SerializeField] private Button setNewCharacterButton;

    private List<CharacterElementUI> characterElementUIList;

    private int selectedCharacterIndex;
    private int currentCharacterIndex;

    private void Awake()
    {
        characterElementUIList = new List<CharacterElementUI>();

        setNewCharacterButton.onClick.AddListener(SetNewCharacterButton_onClick);
        HashDataManager.Instance.OnFriendDataChanged += HashDataManager_OnPlayerDataChanged;

        for (int i = 0; i < lobbyCharacterSOList.List.Count; i++)
        {
            characterElementUIList.Add(CreateCharacterElement(lobbyCharacterSOList.List[i], i));
        }

        UpdateVisual();
    }

    private void SetNewCharacter()
    {
        setNewCharacterButton.gameObject.SetActive(false);

        currentCharacterIndex = selectedCharacterIndex;

        CharacterData characterData = HashDataManager.Instance.GetMyCharacterData();
        characterData.CharacterIndex = currentCharacterIndex;
        HashDataManager.Instance.SetMyCharacterData(characterData).Start();
    }

    private void TryLoadCurrentCharacter()
    {
        currentCharacterIndex = HashDataManager.Instance.GetMyCharacterData().CharacterIndex;
        selectedCharacterIndex = currentCharacterIndex;

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        setNewCharacterButton.gameObject.SetActive(selectedCharacterIndex != currentCharacterIndex);

        for (int i = 0; i < characterElementUIList.Count; i++)
        {
            characterElementUIList[i].SetSelected(selectedCharacterIndex == i);
        }

        characterViewUI.SetNewCharacter(selectedCharacterIndex);
    }

    private void CharacterElementUI_OnClick(int characterIndex)
    {
        selectedCharacterIndex = characterIndex;
        UpdateVisual();
    }

    private void SetNewCharacterButton_onClick() => SetNewCharacter();

    private void HashDataManager_OnPlayerDataChanged() => TryLoadCurrentCharacter();

    private CharacterElementUI CreateCharacterElement(LobbyCharacterSO lobbyCharacterSO, int characterIndex)
    {
        return characterElementFactory.CreateCharacterElement(characterContainer)
                                      .Initialize(lobbyCharacterSO.characterSprite, lobbyCharacterSO.characterName, characterIndex, CharacterElementUI_OnClick);
    }

    private void OnEnable()
    {
        TryLoadCurrentCharacter();
    }

    private void OnDestroy()
    {
        if (HashDataManager.Instance != null)
        {
            HashDataManager.Instance.OnFriendDataChanged -= HashDataManager_OnPlayerDataChanged;
        }
    }
}
