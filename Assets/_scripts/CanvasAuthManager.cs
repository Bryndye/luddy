using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasAuthManager : MonoBehaviour
{
    public static CanvasAuthManager Instance;
    private AuthManager _authManager;

    public TMP_InputField EmailInput;
    public TMP_InputField PasswordInput;
    public Button SignInButton;
    public Button SignUpButton;
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

        // Valeurs test
        EmailInput.text = "test@example.com";
        PasswordInput.text = "Password123!";
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
    public void AddNewProfil()
    {
        Debug.Log("New Profil");
    }
    #endregion
}