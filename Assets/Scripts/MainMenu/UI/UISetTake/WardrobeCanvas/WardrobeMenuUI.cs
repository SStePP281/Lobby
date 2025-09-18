using System.Collections.Generic;
using Unity.VisualScripting;
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

    private void Awake()
    {
        characterElementUIList = new List<CharacterElementUI>();

        setNewCharacterButton.onClick.AddListener(SetNewCharacterButton_onClick);

        for (int i = 0; i < lobbyCharacterSOList.List.Count; i++)
        {
            characterElementUIList.Add(CreateCharacterElement(lobbyCharacterSOList.List[i], i));
        }

        characterViewUI.SetNewCharacter(selectedCharacterIndex);

        HashDataManager.Instance.OnPlayerDataChanged += HashDataManager_OnPlayerDataChanged;
    }

    private void SetNewCharacter()
    {
        //Go to has manager
    }

    private void TryLoadCurrentCharacter()
    {

    }

    private void CharacterElementUI_OnClick(int characterIndex)
    {
        selectedCharacterIndex = characterIndex;

        characterViewUI.SetNewCharacter(selectedCharacterIndex);
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
}
