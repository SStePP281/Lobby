using UnityEngine;
using UnityEngine.UI;

public class LobbyLeaveFromLobbyUI : MonoBehaviour
{
    [SerializeField] private Button leaveButton;

    private void Start()
    {
        LobbyServiceManager.Instance.OnLobbyDataUpdate += LobbyManager_OnLobbyDataUpdate;

        //leaveButton.onClick.AddListener(LobbyServiceManager.Instance.LeaveFromLobby);

        Hide();
    }

    private void LobbyManager_OnLobbyDataUpdate()
    {
        //if (LobbyServiceManager.Instance.IsCanLeaveFromLobby())
        //{
        //    Show();
        //}
        //else
        //{
        //    Hide();
        //}    
    }

    private void OnDestroy()
    {
        LobbyServiceManager.Instance.OnLobbyDataUpdate -= LobbyManager_OnLobbyDataUpdate;
    }
 
    private void Show() => gameObject.SetActive(true);

    private void Hide() => gameObject.SetActive(false);
}
