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
    public AuthManager AuthManager;

    [SerializeField] private Account myAccount;
    [SerializeField] private ProfilUI profilUIPrefab;
    [SerializeField] private Transform container;
    [SerializeField] private GameObject containerTemp;

    [Header("Form New Profil")]
    [SerializeField] private GameObject formNewProfil;
    private Transform newProfilGO;
    [SerializeField] private TMP_InputField inputFieldNameProfil;


    private void Awake()
    {
        formNewProfil.SetActive(false);
    }

    private void Start()
    {
        AuthManager = AuthManager.Instance;
        _canvasAuthManager = CanvasAuthManager.Instance;
    }

    public async void SetSubAccountsProfilsUI(Account account)
    {
        // Obligation d'activer les gameObjects pour pouvoir appeler les fonctions de coroutine
        gameObject.SetActive(true);
        containerTemp.SetActive(true);

        BeforeActiveProfils();
        await AuthManager.MyAccount.LoadAllDatas();
        myAccount = AuthManager.MyAccount;
        AfterGetProfils();
    }
    
    private void BeforeActiveProfils()
    {
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
                            newProfilGO = child;
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
            newProfilGO = container.GetChild(0);
        }
        newProfilGO.SetAsFirstSibling();
        newProfilGO.gameObject.SetActive(false);
    }

    private void AfterGetProfils()
    {
        containerTemp.SetActive(false);

        // Pas de nouveau profil disponible
        // Sauf si le nombre max de subAccount est atteint
        newProfilGO.gameObject.SetActive(myAccount.SubAccounts.Count < myAccount.MaxSubAccounts);

        foreach (SubAccount subAccount in myAccount.SubAccounts)
        {
            var profil = Instantiate(profilUIPrefab, container);
            profil.SetProfil(subAccount);

            // Ajout de l'événement de clic
            profil.TryGetComponent(out Button _btn);
            _btn.onClick.AddListener(() => _canvasAuthManager.ActiveProfil(subAccount));
            profil.DeleteButton.onClick.AddListener(() => DeleteProfil(subAccount));
        }
        newProfilGO?.SetAsLastSibling();
    }


    // Form New Profil UI
    public void AddNewProfil()
    {
        if (Validators.IsValidName(inputFieldNameProfil.text))
        {
            AuthManager.AddNewProfil(inputFieldNameProfil.text);
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
        _canvasAuthManager.Wizard.ActiveWizard(() =>
        {
            AuthManager.DeleteProfil(subAccount);
            Debug.Log(AuthManager.MyAccount.SubAccounts.Count);
            BeforeActiveProfils();
            AfterGetProfils();
            //SetSubAccountsProfilsUI(null);
        }, "Attention !", "Voulez-vous vraiment supprimer le profil "+subAccount.Nom+" ? \nToutes les données liées à ce profil seront supprimées et ne pourront plus être récupérées.");
    }
}