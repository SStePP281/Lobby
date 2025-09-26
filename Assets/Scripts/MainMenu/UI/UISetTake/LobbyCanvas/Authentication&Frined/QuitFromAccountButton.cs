using UnityEngine;
using UnityEngine.UI;

public class QuitFromAccountButton : SystemWindowTrigger
{
    [Header("Reference")]
    [SerializeField] private Button quitFromAccountButton;

    protected override void Awake()
    {
        base.Awake();

        quitFromAccountButton.onClick.AddListener(QuitFromAccountButton_onClick);
    }

    private void QuitFromAccountButton_onClick()
    {
        AuthenticationServiceManager.Instance.TrySignOut();
        window.Close();
    }
}