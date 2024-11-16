using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfilUI : MonoBehaviour
{
    private AuthManager _authManager;

    private SubAccount mySubAccount;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI playerName;
    public bool IsNewProfilUI = false;

    private void Awake()
    {
        _authManager = AuthManager.Instance;
    }

    public void SetProfil(SubAccount subAccount)
    {
        mySubAccount = subAccount;

        playerName.text = subAccount.Nom;
        //this.image.sprite = null;
    }

    public void UseProfil()
    {
        _authManager.SignInSubAccount(mySubAccount);
    }
}
