using Luddy.Validators;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfilsUI : MonoBehaviour
{
    private CanvasAuthManager _canvasAuthManager;
    private AuthManager _authManager;

    [SerializeField] private Account Account;
    [SerializeField] private ProfilUI profilUIPrefab;
    [SerializeField] private Transform container;
    [SerializeField] private GameObject containerTemp;

    [Header("Form New Profil")]
    [SerializeField] private GameObject formNewProfil;
    [SerializeField] private TMP_InputField inputFieldNameProfil;


    private void Awake()
    {
        formNewProfil.SetActive(false);
    }

    private void Start()
    {
        _authManager = AuthManager.Instance;
        _canvasAuthManager = CanvasAuthManager.Instance;
    }

    public void ActiveProfilsUIAccount(Account account)
    {
        gameObject.SetActive(true);
        containerTemp.SetActive(true);
        StartCoroutine(ActivateAccountWithDelay(account, 0.8f)); // Délai d'attente de 1 seconde
    }

    private IEnumerator ActivateAccountWithDelay(Account account, float delay)
    {
        Account = account;

        Transform newProfilT = null;
        if (container.childCount > 1)
        {

            for (int i = 0; i < container.childCount; i++)
            {
                try
                {
                    Transform child = container.GetChild(i);

                    if (child.TryGetComponent(out ProfilUI _pUI))
                    {
                        if (_pUI.IsNewProfilUI)
                        {
                            newProfilT = child;
                            continue;
                        }
                    }

                    Destroy(child.gameObject);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Erreur lors de la destruction de l'enfant {container.GetChild(i).name}: {ex.Message}");
                }
            }
        }
        else
        {
            // Il doit toujours avoir un enfant en initialisation
            newProfilT = container.GetChild(0);
        }
        newProfilT.SetAsFirstSibling();
        newProfilT.gameObject.SetActive(false);

        // Forcer un délai avant d'activer le profil
        yield return new WaitForSeconds(delay);

        newProfilT.gameObject.SetActive(true);
        containerTemp.SetActive(false);

        // Sauf si le nombre max de subAccount est atteint
        if (Account.SubAccounts.Count >= Account.MaxSubAccounts)
        {
            // Pas de nouveau profil disponible
            Destroy(newProfilT.gameObject);
        }

        foreach (SubAccount subAccount in Account.SubAccounts)
        {
            var profil = Instantiate(profilUIPrefab, container);
            profil.SetProfil(subAccount);

            // Ajout de l'événement de clic
            profil.TryGetComponent(out Button _btn);
            _btn.onClick.AddListener(() => _canvasAuthManager.ActiveProfil(subAccount));
        }
        newProfilT?.SetAsLastSibling();
    }

    // Form New Profil UI
    public void AddNewProfil()
    {
        if (Validators.IsValidName(inputFieldNameProfil.text))
        {
            _authManager.AddNewProfil(inputFieldNameProfil.text);
        }
        else
        {
            // Message d'erreur
            Debug.LogError("Nom de profil invalide");
        }
    }
}