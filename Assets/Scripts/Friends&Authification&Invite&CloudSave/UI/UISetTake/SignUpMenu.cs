using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignUpMenu : SystemWindowTrigger
{
    [Header("Reference")]
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordOneField;
    [SerializeField] private TMP_InputField passwordTwoField;
    [SerializeField] private TMP_InputField playerNameField;
    [SerializeField] private Button signUpButton;

    protected override void Awake()
    {
        base.Awake();

        usernameField.characterLimit = AuthenticationServiceData.MAX_USER_NAME_LENGHT;
        passwordOneField.characterLimit = AuthenticationServiceData.MAX_PASSWORD_LENGHT;
        passwordTwoField.characterLimit = AuthenticationServiceData.MAX_PASSWORD_LENGHT;
        playerNameField.characterLimit = AuthenticationServiceData.MAX_PLAYER_NAME_LENGHT;
        signUpButton.onClick.AddListener(SignUpButton_OnClick);

        AuthenticationServiceManager.Instance.OnSignIn += AuthenticationServiceManager_OnSignIn;
    }

    private void AuthenticationServiceManager_OnSignIn() => window.Close();

    private void SignUpButton_OnClick()
    {
        if (passwordOneField.text != passwordTwoField.text)
        {
            InformationUI.Instance.Show("Passwords do not match");
            return;
        }

        if (!AuthenticationServiceManager.Instance.CheckValidPassword(passwordOneField.text))
        {
            InformationUI.Instance.Show(AuthenticationServiceManager.Instance.GetPasswordValidParam());
            return;
        }

        if (!AuthenticationServiceManager.Instance.CheckValidUsername(usernameField.text))
        {
            InformationUI.Instance.Show(AuthenticationServiceManager.Instance.GetUsernameValidParam());
            return;
        }

        if (!AuthenticationServiceManager.Instance.CheckValidPlayerName(playerNameField.text))
        {
            InformationUI.Instance.Show(AuthenticationServiceManager.Instance.GetPlayerNameValidParam());
            return;
        }

        if (AuthenticationServiceManager.Instance.TrySingUp(usernameField.text, passwordOneField.text, playerNameField.text, AuthenticationServiceManager_TrySingUp_Callback))
        {
            InformationUI.Instance.Show("Wait...", false);
        }
        else
        {
            InformationUI.Instance.Show(AuthenticationServiceManager.Instance.GetPasswordValidParam() +
                                        AuthenticationServiceManager.Instance.GetUsernameValidParam() + 
                                        AuthenticationServiceManager.Instance.GetPlayerNameValidParam());
        }
    }

    private void AuthenticationServiceManager_TrySingUp_Callback(bool succes, string errorMessage)
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

    private void OnDisable()
    {
        usernameField.text = string.Empty;
        passwordOneField.text = string.Empty;
        passwordTwoField.text = string.Empty;
        playerNameField.text = string.Empty;
    }

    private void OnDestroy()
    {
        if (AuthenticationServiceManager.Instance != null)
        {
            AuthenticationServiceManager.Instance.OnSignIn -= AuthenticationServiceManager_OnSignIn;
        }
    }
}
