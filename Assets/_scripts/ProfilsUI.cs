using Luddy.Validators;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfilsUI : MonoBehaviour
{
    private CanvasAuthManager _canvasAuthManager;
    private AuthManager _authManager;

    [SerializeField] private Account myAccount;
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

    public void ActivateAccountWithDelay(Account account)
    {
        // Obligation d'activer les gameObjects pour pouvoir appeler les fonctions de coroutine
        gameObject.SetActive(true);
        containerTemp.SetActive(true);

        StartCoroutine(ActiveProfilsUIAccount(account, 0.8f)); // Délai d'attente de 1 seconde
    }

    private void ActiveProfilsUIAccountWithoutDelay()
    {
        // Obligation d'activer les gameObjects pour pouvoir appeler les fonctions de coroutine
        gameObject.SetActive(true);
        containerTemp.SetActive(true);

        StartCoroutine(ActiveProfilsUIAccount(myAccount, 0));
    }
    
    private IEnumerator ActiveProfilsUIAccount(Account account, float delay)
    {
        myAccount = account;

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
                        }
                        else
                        {
                            Destroy(child.gameObject);
                        }
                    }

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

        containerTemp.SetActive(false);

        // Pas de nouveau profil disponible
        // Sauf si le nombre max de subAccount est atteint
        newProfilT.gameObject.SetActive(myAccount.SubAccounts.Count < myAccount.MaxSubAccounts);

        foreach (SubAccount subAccount in myAccount.SubAccounts)
        {
            var profil = Instantiate(profilUIPrefab, container);
            profil.SetProfil(subAccount);

            // Ajout de l'événement de clic
            profil.TryGetComponent(out Button _btn);
            _btn.onClick.AddListener(() => _canvasAuthManager.ActiveProfil(subAccount));
            profil.DeleteButton.onClick.AddListener(() => DeleteProfil(subAccount));
        }
        newProfilT?.SetAsLastSibling();
    }

    // Form New Profil UI
    public void AddNewProfil()
    {
        if (Validators.IsValidName(inputFieldNameProfil.text))
        {
            _authManager.AddNewProfil(inputFieldNameProfil.text);
            _canvasAuthManager.ActiveProfil(myAccount.SubAccounts.Last());
        }
        else
        {
            // Message d'erreur
            Debug.LogError("Nom de profil invalide");
        }
    }

    public void DeleteProfil(SubAccount subAccount)
    {
        _authManager.DeleteProfil(subAccount);
        ActiveProfilsUIAccountWithoutDelay();
    }
}