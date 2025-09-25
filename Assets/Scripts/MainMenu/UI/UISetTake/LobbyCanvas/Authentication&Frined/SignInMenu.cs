using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ServiceRequest;

public class SignInMenu : SystemWindowTrigger
{
    [Header("Reference")]
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private Button signInButton;

    protected override void Awake()
    {
        base.Awake();

        usernameField.characterLimit = AuthenticationServiceData.MAX_USER_NAME_LENGTH;
        passwordField.characterLimit = AuthenticationServiceData.MAX_USER_NAME_LENGTH;
        signInButton.onClick.AddListener(SignInButton_OnClick);

        AuthenticationServiceManager.Instance.OnSignIn += AuthenticationServiceManager_OnSignIn;
        AuthenticationServiceManager.Instance.OnSignOut += AuthenticationServiceManager_OnSignOut;
    }

    private void AuthenticationServiceManager_OnSignOut() => window.Open();

    private void AuthenticationServiceManager_OnSignIn() => window.Close();

    private void SignInButton_OnClick()
    {
        AuthenticationServiceManager.Instance.TrySignIn(usernameField.text, passwordField.text)
            .OnStartRun(() => InformationUI.Instance.Show("Wait...", false))
            .OnSuccessComplete(() => InformationUI.Instance.Hide())
            .OnFailureComplete(m => InformationUI.Instance.Show(m))
            .Start();
    }

    private void OnDisable()
    {
        usernameField.text = string.Empty;
        passwordField.text = string.Empty;
    }

    private void OnDestroy()
    {
        if (AuthenticationServiceManager.Instance != null)
        {
            AuthenticationServiceManager.Instance.OnSignIn -= AuthenticationServiceManager_OnSignIn;
            AuthenticationServiceManager.Instance.OnSignOut -= AuthenticationServiceManager_OnSignOut;
        }
    }
}
