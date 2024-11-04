using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using NaughtyAttributes;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance;

    private CanvasAuthManager _authManager;
    [SerializeField] private GameObject _mainCanvas;

    [BoxGroup("Accounts")]
    public Account MyAccount;


    #region Init
    async void Awake()
    {
        Instance = this;

        try
        {
            await UnityServices.InitializeAsync();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        SetupEvents();
    }

    // Setup authentication event handlers if desired
    void SetupEvents()
    {
        AuthenticationService.Instance.SignedIn += () => {
            // Shows how to get a playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            // Shows how to get an access token
            Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");

            // Desactive auth canvas and active main canvas
            _authManager.gameObject.SetActive( false );
            _mainCanvas.SetActive( true );
        };

        AuthenticationService.Instance.SignInFailed += (err) => {
            Debug.LogError(err);
        };

        AuthenticationService.Instance.SignedOut += () => {
            Debug.Log("Player signed out.");
        };

        AuthenticationService.Instance.Expired += () =>
        {
            Debug.Log("Player session could not be refreshed and expired.");
        };
    }

    private void Start()
    {
        _authManager = CanvasAuthManager.Instance;

        _authManager.OnSignIn += SignIn;
        _authManager.OnSignUp += SignUp;
    }
    #endregion

    #region Validators
    bool ValidateInputs(string email, string password)
    {
        return !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password);
    }

    public bool IsValidEmail(string email)
    {
        string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, emailPattern);
    }
    #endregion

    #region Sign Up/in - Edit Account
    public async void SignUp(string email, string password)
    {
        if (!ValidateInputs(email, password))
        {
            Debug.Log("AuthManager/ ERR : missing username or password : " + email+ " " + password);
            return;
        }
        if (!IsValidEmail(email))
        {
            Debug.Log("AuthManager/ ERR : wrong format email : " + email);
            return;
        }
        await SignUpWithUsernamePasswordAsync(email, password);
        UpdateCurrentAccount();
    }
    async Task SignUpWithUsernamePasswordAsync(string email, string password)
    {
        try
        {
            Debug.Log(email);
            Debug.Log(password);
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(email, password);
            Debug.Log("AuthManager : SignUp is successful.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    public async void SignIn(string email, string password)
    {
        if (!ValidateInputs(email, password))
        {
            Debug.Log("AuthManager/ ERR : missing username or password " + email + " " + password);
            return;
        }
        if (!IsValidEmail(email))
        {
            Debug.Log("AuthManager/ ERR : wrong format email : " + email);
            return;
        }
        await SignInWithUsernamePasswordAsync(email, password);
        UpdateCurrentAccount();
    }
    async Task SignInWithUsernamePasswordAsync(string email, string password)
    {
        Debug.Log(email + password);
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(email, password);
            Debug.Log("AuthManager : SignIn is successful.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    async Task UpdatePasswordAsync(string currentPassword, string newPassword)
    {
        try
        {
            await AuthenticationService.Instance.UpdatePasswordAsync(currentPassword, newPassword);
            Debug.Log("AuthManager : Password updated.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }
    #endregion

    public void UpdateCurrentAccount()
    {
        MyAccount = new Account(AuthenticationService.Instance.PlayerId);
        Debug.Log("AuthManager : new current account");
    }
}