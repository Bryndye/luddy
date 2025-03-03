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
    public Wizard Wizard;
    public GameObject incorrectId;

    public Action<string, string> OnSignInUI;
    public Action<string, string> OnSignUpUI;
    public Action OnSelectedProfilUI;

    private void Awake()
    {
        Instance = this;

        SignInButton.onClick.AddListener(SignInUI);
        SignUpButton.onClick.AddListener(SignUpUI);
        SignInButton.onClick.AddListener(() => SaveIdentifiants(EmailInput.text, PasswordInput.text));
        SignUpButton.onClick.AddListener(() => SaveIdentifiants(EmailInput.text, PasswordInput.text));

        // Password Type
        PasswordInput.contentType = TMP_InputField.ContentType.Password;
        PasswordInput.asteriskChar = '*';

        incorrectId.SetActive(false);
    }

    private void Start()
    {
        _authManager = AuthManager.Instance;

        // Permet d'activer l'écran des profils
        _authManager.OnSignIn += MyProfilsUI.SetSubAccountsProfilsUI;
        _authManager.OnSubAccountSignIn += SetActiveProfilsUIFalse;

        MyProfilsUI.AuthManager = _authManager;

        // Valeurs test
        EmailInput.text = "test@example.com";
        PasswordInput.text = "Password123!";


        AuthenticationService.Instance.SignedIn += () =>
        {
            AuthUI?.SetActive(false);
        };

        AuthenticationService.Instance.SignInFailed += (err) =>
        {
            //AuthUI?.SetActive(true);
            incorrectId.SetActive(true);
            MyProfilsUI.gameObject.SetActive(false);
        };

        AuthenticationService.Instance.SignedOut += () =>
        {
            AuthUI?.SetActive(true);
        };

        // TitleScreen arrive en premier
        gameObject.SetActive(false);

        SetIdentifiants();
    }

    public void ActiveAuthentification(Action callback)
    {
        // Permet d'activer l'écran d'authentification
        // Et d'appeler des functions une fois l'écran terminé
        OnSelectedProfilUI = null; // Nettoie les actions précédentes
        OnSelectedProfilUI = callback;

        // Gestion des écrans
        gameObject.SetActive(true);
        MyProfilsUI.gameObject.SetActive(false);
    }

    public void ActiveSelectionProfil(Action callback)
    {
        // Permet d'activer l'écran d'authentification
        // Et d'appeler des functions une fois l'écran terminé
        OnSelectedProfilUI = null; // Nettoie les actions précédentes
        OnSelectedProfilUI = callback;

        // Gestion des écrans
        gameObject.SetActive(true);
        MyProfilsUI.gameObject.SetActive(true);
        MyProfilsUI.SetSubAccountsProfilsUI(_authManager.MyAccount);
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

    private void SaveIdentifiants(string email, string password)
    {
        if (string.IsNullOrEmpty(email)) { return; }

        PlayerPrefs.SetString("email", email);
        PlayerPrefs.SetString("password", password);
    }

    private void SetIdentifiants()
    {
        string _e = PlayerPrefs.GetString("email");
        string _p = PlayerPrefs.GetString("password");
        if (_e == null)
            return;
        EmailInput.text = PlayerPrefs.GetString("email");
        PasswordInput.text = PlayerPrefs.GetString("password");
    }


    #region Profils
    public void SetActiveProfilsUIFalse()
    {
        MyProfilsUI.gameObject.SetActive(false);
    }

    public void ActiveProfil(int id)
    {
        _authManager.SignInSubAccount(id);
        OnSelectedProfilUI?.Invoke();
        gameObject.SetActive(false);
    }
    #endregion
}