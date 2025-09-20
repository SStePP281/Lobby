using ServiceRequest;
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
        InformationUI.Instance.Show("Wait...", false);

        AuthenticationServiceManager.Instance.TrySingUp(usernameField.text, passwordOneField.text, passwordTwoField.text, playerNameField.text)
                                             .OnSuccessComplete(() => { InformationUI.Instance.Hide(); window.Close(); })
                                             .OnFailureComplete(m => InformationUI.Instance.Show(m))
                                             .Start();
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
