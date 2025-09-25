using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerSetup : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Transform modelContainer;
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private Image hostImage;
    [SerializeField] private Image readyImage;

    public string CliendID { get; private set; }
    public int CharacterIndex { get; private set; }

    private Transform currentModel;

    public LobbyPlayerSetup Initialize(LobbyUserData lobbyUserData, Transform model)
    {
        CliendID = lobbyUserData.UserID;

        SetModel(lobbyUserData.CharacterIndex, model);
        SetNickname(lobbyUserData.Nickname);
        SetHostFlag(lobbyUserData.IsHost);
        SetReadyFlag(lobbyUserData.IsReady);

        return this;
    }

    public void SetModel(int characterIndex, Transform model)
    {
        CharacterIndex = characterIndex;

        if (currentModel != null)
        {
            Destroy(currentModel.gameObject);
        }

        currentModel = model;
    }

    public void SetNickname(string nickname) => nicknameText.text = nickname;

    public void SetReadyFlag(bool isReady) => readyImage.gameObject.SetActive(isReady);

    public void SetHostFlag(bool isHost) => hostImage.gameObject.SetActive(isHost);

    public Transform GetModelContainer() => modelContainer;
}