using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyJoinUI : MonoBehaviour
{
    [Header("Take")]
    [SerializeField] private TMP_InputField lobbyCode;
    [SerializeField] private Outline lobbyCodeOutline;
    [SerializeField] private TMP_InputField lobbyPassword;
    [SerializeField] private Outline lobbyPasswordOutline;
    [SerializeField] private Button tryJoinButton;

    [Header("Set")]
    [SerializeField] private Color correctData;
    [SerializeField] private Color invalidData;

    private void Awake()
    {
        lobbyCode.characterLimit = LobbyData.maxLobbyCodeLenght;
        lobbyPassword.characterLimit = LobbyData.maxPasswordLenght;

        tryJoinButton.onClick.AddListener(TryJoinToLobby);
    }

    private void TryJoinToLobby()
    {
        //if (LobbyServiceManager.Instance.JoinLobbyByCode(lobbyCode.text, lobbyPassword.text, LobbyManager_OnJoinLobbyResult))
        //{
        //    lobbyCodeOutline.effectColor = correctData;
        //    lobbyPasswordOutline.effectColor = correctData;

        //    LobbyProcessBackgroundUI.Instance.ShowWaitConnect();
        //}
        //else
        //{
        //    lobbyCodeOutline.effectColor = invalidData;
        //    lobbyPasswordOutline.effectColor = invalidData;
        //}
    }

    private void LobbyManager_OnJoinLobbyResult(bool isSuccess)
    {
        LobbyProcessBackgroundUI.Instance.HideAll(true);

        if (!isSuccess)
        {
            LobbyProcessBackgroundUI.Instance.ShowFaildConnect();
        }
    }
}
