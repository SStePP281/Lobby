using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyGameDataUI : MonoBehaviour
{
    [SerializeField] private SwitchedButtonUI readyButton;
    [SerializeField] private SwitchedButtonUI onlineButton;
    [SerializeField] private Button currnetMapButton;
    [SerializeField] private TextMeshProUGUI currentMapName;

    private void Awake()
    {
        readyButton.OnChangeActive.AddListener(LobbyGameManager.Instance.SetClientReady);
        onlineButton.OnChangeActive.AddListener(LobbyGameManager.Instance.SetGameType);
    }

    private void Start()
    {
        LobbyGameManager.Instance.OnCurrentMapChenged += LobbyGameManager_OnCurrentMapChenged;
        LobbyGameManager.Instance.OnGameTypeChenged += LobbyGameManager_OnGameTypeChenged;
        LobbyServiceManager.Instance.OnLobbyGenerealDataUpdate += LobbyManager_OnLobbyGenerealDataUpdate;

        LobbyGameManager_OnCurrentMapChenged();
        LobbyGameManager_OnGameTypeChenged();
    }

    private void LobbyGameManager_OnGameTypeChenged()
    {
        onlineButton.SetState(LobbyGameManager.Instance.IsOnlineGame());
    }

    private void LobbyGameManager_OnCurrentMapChenged()
    {
        LobbyMapDataSO lobbyMapDataSO = LobbyGameManager.Instance.GetCurrentLobbbyMapDataSo();
        currnetMapButton.image.sprite = lobbyMapDataSO.sceneView;
        currentMapName.text = lobbyMapDataSO.sceneViewName;
    }

    private void LobbyManager_OnLobbyGenerealDataUpdate()
    {
        //currnetMapButton.enabled = LobbyServiceManager.Instance.IsMyLobby();
        //onlineButton.Button.enabled = LobbyServiceManager.Instance.IsMyLobby();
    }

    private void OnDestroy()
    {
        LobbyGameManager.Instance.OnCurrentMapChenged -= LobbyGameManager_OnCurrentMapChenged;
        LobbyGameManager.Instance.OnGameTypeChenged -= LobbyGameManager_OnGameTypeChenged;
        LobbyServiceManager.Instance.OnLobbyGenerealDataUpdate -= LobbyManager_OnLobbyGenerealDataUpdate;
    }
}
