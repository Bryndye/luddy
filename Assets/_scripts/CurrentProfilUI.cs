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
        Debug.Log("Start");
    }
    
    public void SetContent()
    {
        profilName.text = authManager.MyCurrentSubAccount.Nom.ToString();
    }


    public void ActiveParameter()
    {
        parameter.SetActive(!parameter.activeSelf);
    }
}