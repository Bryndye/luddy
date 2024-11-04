using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasAuthManager : MonoBehaviour
{
    public static CanvasAuthManager Instance;

    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button signInButton;
    public Button signUpButton;

    public Action<string, string> OnSignIn;
    public Action<string, string> OnSignUp;

    private void Awake()
    {
        Instance = this;

        signInButton.onClick.AddListener(OnSignInUI);
        signUpButton.onClick.AddListener(OnSignUpUI);
    }

    private void Start()
    {
        emailInput.text = "test@example.com";
        passwordInput.text = "Password123!";
    }

    void OnSignInUI()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        OnSignIn?.Invoke(email, password);
    }

    void OnSignUpUI()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        OnSignUp?.Invoke(email, password);
    }

}