using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AuthenticationMenuUI : MonoBehaviour
{
    [Header("Sign in/up panel")]
    [SerializeField] private Transform signPanel;
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private Button signInButton;
    [SerializeField] private Button signUpButton;

    [Header("Player name panel")]
    [SerializeField] private Transform playerNamePanel;
    [SerializeField] private TMP_InputField playerNameField;
    [SerializeField] private Button setPlayerNameButton;

    private bool isRegister = true;

    private void Awake()
    {
        signInButton.onClick.AddListener(SignInButton_OnClick);
        signUpButton.onClick.AddListener(SignUpButton_OnClick);
        setPlayerNameButton.onClick.AddListener(SetPlayerNameButton_OnClick);

        Show();

        AuthenticationServiceManager.Instance.OnSignIn += AuthenticationManager_OnSignIn;
        AuthenticationServiceManager.Instance.OnSignOut += AuthenticationManager_OnSindOut;
    }

    private void SignInButton_OnClick()
    {
        if (AuthenticationServiceManager.Instance.TrySingIn(usernameField.text, passwordField.text, AuthenticationServiceManager_TrySingIn_Callback))
        {
            InformationUI.Instance.Show("Try sign in...", false);

            isRegister = true;
        }
        else
        {
            InformationUI.Instance.Show(AuthenticationServiceManager.Instance.GetPasswordValidParam() + "\n" + AuthenticationServiceManager.Instance.GetUsernameValidParam());
        }
    }

    private void SignUpButton_OnClick()
    {
        if (AuthenticationServiceManager.Instance.TrySingUp(usernameField.text, passwordField.text, AuthenticationServiceManager_TrySingUp_Callback))
        {
            InformationUI.Instance.Show("Try sign up...", false);

            isRegister = false;
        }
        else
        {
            InformationUI.Instance.Show(AuthenticationServiceManager.Instance.GetPasswordValidParam() + "\n" + AuthenticationServiceManager.Instance.GetUsernameValidParam());
        }
    }

    private void SetPlayerNameButton_OnClick()
    {

        if (AuthenticationServiceManager.Instance.TrySetNewPlayrName(playerNameField.text, AuthenticationServiceManager_TrySetNewPlayrName_Callback))
        {
            InformationUI.Instance.Show("Try set name...", false);
        }
        else
        {
            InformationUI.Instance.Show(AuthenticationServiceManager.Instance.GetPlayerNameValidParam());
        }
    }

    private void AuthenticationServiceManager_TrySetNewPlayrName_Callback(bool succes, string errorMessage)
    {
        if (succes)
        {
            Hide();

            InformationUI.Instance.Hide();
        }
        else
        {
            InformationUI.Instance.Show(errorMessage);
        }
    }

    private void AuthenticationServiceManager_TrySingIn_Callback(bool succes, string errorMessage)
    {
        if (succes)
        {
            InformationUI.Instance.Hide();

            Hide();
        }
        else
        {
            InformationUI.Instance.Show(errorMessage);
        }    
    }

    private void AuthenticationServiceManager_TrySingUp_Callback(bool succes, string errorMessage)
    {
        if (succes)
        {
            InformationUI.Instance.Hide();

            Show();
        }
        else
        {
            InformationUI.Instance.Show(errorMessage);
        }
    }

    private void AuthenticationManager_OnSignIn()
    {
        if (isRegister)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    private void AuthenticationManager_OnSindOut()
    {
        isRegister = true;

        Show();
    }

    private void Show()
    {
        InformationUI.Instance.Hide();

        passwordField.text = string.Empty;
        usernameField.text = string.Empty;
        playerNameField.text = string.Empty;

        gameObject.SetActive(true);
        signPanel.gameObject.SetActive(isRegister);
        playerNamePanel.gameObject.SetActive(!isRegister);
    }

    private void Hide()
    {
        InformationUI.Instance.Hide();

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (AuthenticationServiceManager.Instance != null)
        {
            AuthenticationServiceManager.Instance.OnSignIn -= AuthenticationManager_OnSignIn;
            AuthenticationServiceManager.Instance.OnSignOut -= AuthenticationManager_OnSindOut;
        }
    }
}
