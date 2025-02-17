using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrentProfilUI : MonoBehaviour
{
    private AuthManager authManager;
    [SerializeField] private TextMeshProUGUI profilName;
    [SerializeField] private Image profilIcon;
    [SerializeField] private GameObject parameter;

    private void Start()
    {
        authManager = AuthManager.Instance;

        SetContent();

        // Forcer la desactivation de la page
        parameter.SetActive(false);
    }
    
    public void SetContent()
    {
        bool isSubscribed = authManager.MyAccount.isSubscribed;
        string sub = isSubscribed ? "\n Abonné" : "";
        profilName.text = authManager.MyCurrentSubAccount.Nom.ToString() + sub;
    }


    public void ActiveParameter()
    {
        parameter.SetActive(!parameter.activeSelf);
    }
}