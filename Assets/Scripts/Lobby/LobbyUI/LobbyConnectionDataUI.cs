using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyConnectionDataUI : MonoBehaviour
{
    //private const string LOBBY_CODE_START = "Lobby code: ";

    //[Header("Take")]
    //[SerializeField] private TextMeshProUGUI lobbyCodeTMP;
    //[SerializeField] private TMP_InputField passwordField;
    //[SerializeField] private Outline passwordOutline;
    //[SerializeField] private Outline clientNameOutline;
    //[SerializeField] private TMP_InputField clientNameField;

    //[Header("Set")]
    //[SerializeField] private Color correctData;
    //[SerializeField] private Color invalidData;

    //private void Awake()
    //{
    //    passwordField.characterLimit = LobbyServiceData.maxPasswordLenght;
    //    clientNameField.characterLimit = LobbyServiceData.maxClientNameLenght;

    //    passwordField.onEndEdit.AddListener(TrySetNewPassword);
    //    clientNameField.onEndEdit.AddListener(TrySetNewClientName);
    //}

    //private void Start()
    //{
    //    LobbyServiceManager.Instance.OnLobbyGenerealDataUpdate += LobbyManager_LobbyCode_OnValueChanged;
    //}

    //private void TrySetNewPassword(string newPassword)
    //{
    //    //if (LobbyServiceManager.Instance.SetLobbyPassword(newPassword))
    //    //{
    //    //    passwordOutline.effectColor = correctData;
    //    //}
    //    //else
    //    //{
    //    //    passwordOutline.effectColor = invalidData;
    //    //}
    //}

    //private void TrySetNewClientName(string newClientName)
    //{
    //    //if (LobbyServiceManager.Instance.SetClientName(newClientName))
    //    //{
    //    //    clientNameOutline.effectColor = correctData;
    //    //}
    //    //else
    //    //{
    //    //    clientNameOutline.effectColor = invalidData;
    //    //}
    //}

    //private void LobbyManager_LobbyCode_OnValueChanged()
    //{
    //    //lobbyCodeTMP.text = LOBBY_CODE_START + LobbyServiceManager.Instance.GetLobbyCode();
    //    //clientNameField.text = LobbyServiceManager.Instance.GetClientName();

    //    //if (LobbyServiceManager.Instance.IsMyLobby())
    //    //{
    //    //    ShowPasswordField();
    //    //}
    //    //else
    //    //{
    //    //    HidePasswordField();
    //    //}    
    //}

    //private void ShowPasswordField() => passwordField.gameObject.SetActive(true);

    //private void HidePasswordField() => passwordField.gameObject.SetActive(false);

    //private void OnDestroy()
    //{
    //    LobbyServiceManager.Instance.OnLobbyGenerealDataUpdate -= LobbyManager_LobbyCode_OnValueChanged;
    //}
}
