using System;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class CanvasAuthManager : MonoBehaviour
{
    public static CanvasAuthManager Instance;
    private AuthManager _authManager;

    [Header("Authentification Screen")]
    public GameObject AuthUI;
    public TMP_InputField EmailInput;
    public TMP_InputField PasswordInput;
    public Button SignInButton;
    public Button SignUpButton;

    [Header("Profils Screen")]
    public ProfilsUI MyProfilsUI;

    public Action<string, string> OnSignInUI;
    public Action<string, string> OnSignUpUI;

    private void Awake()
    {
        Instance = this;

        SignInButton.onClick.AddListener(SignInUI);
        SignUpButton.onClick.AddListener(SignUpUI);
    }

    private void Start()
    {
        _authManager = AuthManager.Instance;

        // Permet d'activer l'écran des profils
        _authManager.OnSignIn += MyProfilsUI.ActiveProfilsUIAccount;
        _authManager.OnSubAccountSignIn += SetActiveProfilsUIFalse;


        // Valeurs test
        EmailInput.text = "test@example.com";
        PasswordInput.text = "Password123!";


        AuthenticationService.Instance.SignedIn += () =>
        {
            AuthUI?.SetActive(false);
        };

        AuthenticationService.Instance.SignInFailed += (err) =>
        {
            AuthUI?.SetActive(true);
        };

        AuthenticationService.Instance.SignedOut += () =>
        {
            AuthUI?.SetActive(true);
        };
    }

    void SignInUI()
    {
        string email = EmailInput.text;
        string password = PasswordInput.text;

        OnSignInUI?.Invoke(email, password);
    }

    void SignUpUI()
    {
        string email = EmailInput.text;
        string password = PasswordInput.text;

        OnSignUpUI?.Invoke(email, password);
    }


    #region Profils
    public void SetActiveProfilsUIFalse()
    {
        MyProfilsUI.gameObject.SetActive(false);
    }
    public void SetActiveProfilsUI(bool active)
    {
        MyProfilsUI.gameObject.SetActive(active);
    }
    #endregion
}