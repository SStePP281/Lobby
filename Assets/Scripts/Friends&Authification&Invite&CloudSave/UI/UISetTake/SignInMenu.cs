using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignInMenu : SystemWindowTrigger
{
    [Header("Reference")]
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private Button signInButton;

    protected override void Awake()
    {
        base.Awake();

        usernameField.characterLimit = AuthenticationServiceData.MAX_USER_NAME_LENGHT;
        passwordField.characterLimit = AuthenticationServiceData.MAX_PASSWORD_LENGHT;
        signInButton.onClick.AddListener(SignInButton_OnClick);

        AuthenticationServiceManager.Instance.OnSignIn += AuthenticationServiceManager_OnSignIn;
        AuthenticationServiceManager.Instance.OnSignOut += AuthenticationServiceManager_OnSignOut;
    }

    private void AuthenticationServiceManager_OnSignOut() => window.Open();

    private void AuthenticationServiceManager_OnSignIn() => window.Close();

    private void SignInButton_OnClick()
    {
        if (!AuthenticationServiceManager.Instance.CheckValidUsername(usernameField.text))
        {
            InformationUI.Instance.Show(AuthenticationServiceManager.Instance.GetUsernameValidParam());
            return;
        }

        if (!AuthenticationServiceManager.Instance.CheckValidPassword(passwordField.text))
        {
            InformationUI.Instance.Show(AuthenticationServiceManager.Instance.GetPasswordValidParam());
            return;
        }

        if (AuthenticationServiceManager.Instance.TrySingIn(usernameField.text, passwordField.text, AuthenticationServiceManager_TrySingIn_Callback))
        {
            InformationUI.Instance.Show("Wait...", false);
        }
        else
        {
            InformationUI.Instance.Show(AuthenticationServiceManager.Instance.GetPasswordValidParam() + 
                                        AuthenticationServiceManager.Instance.GetUsernameValidParam());
        }
    }

    private void AuthenticationServiceManager_TrySingIn_Callback(bool succes, string errorMessage)
    {
        if (succes)
        {
            InformationUI.Instance.Hide();
        }
        else
        {
            InformationUI.Instance.Show(errorMessage);
        }
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
