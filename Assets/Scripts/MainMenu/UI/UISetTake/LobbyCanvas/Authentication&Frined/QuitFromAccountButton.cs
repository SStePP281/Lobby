using UnityEngine;
using UnityEngine.UI;

public class QuitFromAccountButton : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Button quitFromAccountButton;

    private void Awake()
    {
        quitFromAccountButton.onClick.AddListener(QuitFromAccountButton_onClick);
    }

    private void QuitFromAccountButton_onClick()
    {
        AuthenticationServiceManager.Instance.TrySignOut();
    }
}