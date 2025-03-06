using Luddy.Validators;
using NaughtyAttributes;
using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance;

    private CanvasAuthManager _canvasAuthManager;
    [SerializeField] private GameObject _mainCanvas;

    [BoxGroup("Accounts")]
    public Account MyAccount;
    public SubAccount MyCurrentSubAccount { get { 
            return MyAccount.SubAccounts[currentSubAccountId]; 
    }}
    private int currentSubAccountId = 0;
    public Action<Account> OnSignIn;
    public Action<Account> OnSignUp;
    public Action OnSubAccountSignIn;


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
        AuthenticationService.Instance.SignInFailed += (err) =>
        {
            Debug.LogError(err);
        };

        AuthenticationService.Instance.SignedOut += () =>
        {
            Debug.Log("Player signed out.");
        };

        AuthenticationService.Instance.Expired += () =>
        {
            Debug.Log("Player session could not be refreshed and expired.");
        };
    }

    private void Start()
    {
        _canvasAuthManager = CanvasAuthManager.Instance;

        _canvasAuthManager.OnSignInUI += SignIn;
        _canvasAuthManager.OnSignUpUI += SignUp;
    }
    #endregion

    #region Sign Up/in - Edit Account
    public async void SignUp(string email, string password)
    {
        if (!Validators.ValidateInputs(email, password))
        {
            Debug.Log("AuthManager/ ERR : missing username or password : " + email + " " + password);
            _canvasAuthManager.IncorrectText("Le mail ou le mot de passe est incorrect");
            return;
        }
        if (!Validators.IsValidEmail(email))
        {
            Debug.Log("AuthManager/ ERR : wrong format email : " + email);
            _canvasAuthManager.IncorrectText("Le format du mail est incorrect");
            return;
        }
        await SignUpWithUsernamePasswordAsync(email, password);
    }
    async Task SignUpWithUsernamePasswordAsync(string email, string password)
    {
        try
        {
            Debug.Log(email);
            Debug.Log(password);
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(email, password);
            Debug.Log("AuthManager : SignUp is successful.");
            UpdateCurrentAccount();
            OnSignIn?.Invoke(MyAccount);
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            //Debug.LogException(ex);
            if (ex.Message.Contains("username"))
            {
                _canvasAuthManager.IncorrectText("Le compte existe déjà, vous ne pouvez pas recréer un compte avec ce même identifiant");
            }
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            //Debug.LogException(ex);

            if (ex.Message.Contains("Password"))
            {
                Debug.Log(ex.Message);
                _canvasAuthManager.IncorrectText("Le mot de passe ne respecte pas les conditions, il faut au moins une majuscule, une minuscule, un chiffre et un symbole.");
            }
        }
    }

    public async void SignIn(string email, string password)
    {
        if (!Validators.ValidateInputs(email, password))
        {
            Debug.Log("AuthManager/ ERR : missing username or password " + email + " " + password);
            _canvasAuthManager.IncorrectText("Le mail ou le mot de passe est incorrect");
            return;
        }
        if (!Validators.IsValidEmail(email))
        {
            Debug.Log("AuthManager/ ERR : wrong format email : " + email);
            _canvasAuthManager.IncorrectText("Le format du mail est incorrect");
            return;
        }
        await SignInWithUsernamePasswordAsync(email, password);

        UpdateCurrentAccount();
        OnSignIn?.Invoke(MyAccount);
    }
    async Task SignInWithUsernamePasswordAsync(string email, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(email, password);
            Debug.Log("AuthManager : SignIn is successful.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
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
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            Debug.LogException(ex);
        }
    }

    public void SignInSubAccount(int id = 0)
    {
        //MyCurrentSubAccount = subAccount ?? subAccount;
        currentSubAccountId = id;
        OnSubAccountSignIn?.Invoke();
    }

    #endregion

    #region Account
    public Account UpdateCurrentAccount()
    {
        MyAccount = new Account(AuthenticationService.Instance.PlayerId);
        return MyAccount;
    }

    public void AddNewProfil(string name = null)
    {
        MyAccount.AddNewProfil(name);
    }

    public void DeleteProfil(SubAccount subAccount)
    {
        MyAccount.DeleteProfil(subAccount);
    }
    #endregion
}