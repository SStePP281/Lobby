using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCharacterGridElementUI : MonoBehaviour
{
    [SerializeField] private Button characterButton;
    [SerializeField] private Outline outline;
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI characterName;

    private LobbyPlayerDataSO lobbyPlayerDataSO;

    private void Awake()
    {
        characterButton.onClick.AddListener(() => { LobbyCharacterManager.Instance.SetNewCharacter(lobbyPlayerDataSO); });
    }

    public void Initialize(LobbyPlayerDataSO lobbyPlayerDataSO, bool isSelected)
    {
        this.lobbyPlayerDataSO = lobbyPlayerDataSO;

        characterImage.sprite = lobbyPlayerDataSO.characterSprite;
        characterName.text = lobbyPlayerDataSO.characterName;

        SetIsSelectedCharacter(isSelected);
    }

    public void SetIsSelectedCharacter(bool isSelected)
    {
        outline.enabled = isSelected;
    }
}
