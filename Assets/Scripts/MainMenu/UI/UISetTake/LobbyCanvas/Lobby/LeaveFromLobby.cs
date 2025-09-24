using UnityEngine;
using UnityEngine.UI;

public class LeaveFromLobby : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Button leaveButton;

    private void Awake()
    {
        leaveButton.onClick.AddListener(LobbyServiceManager.Instance.LeaveFromLobby().Start);
    }
}