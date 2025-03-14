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
    [HideInInspector] public AuthManager AuthManager;

    [SerializeField] private Account myAccount;
    [SerializeField] private ProfilUI profilUIPrefab;
    [SerializeField] private Transform container;
    [SerializeField] private GameObject containerTemp;

    [Header("Form New Profil")]
    [SerializeField] private GameObject formNewProfil;
    private Transform newProfilGO;
    [SerializeField] private TMP_InputField inputFieldNameProfil;
    //bool isLoading = false;


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
        formNewProfil.gameObject.SetActive(false);

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
        formNewProfil.gameObject.SetActive(false);
        container.gameObject.SetActive(true);
        
        // Pas de nouveau profil disponible
        // Sauf si le nombre max de subAccount est atteint
        newProfilGO.gameObject.SetActive(myAccount.SubAccounts.Count < myAccount.MaxSubAccounts);

        int index = 0;
        foreach (SubAccount subAccount in myAccount.SubAccounts)
        {
            var profil = Instantiate(profilUIPrefab, container);
            profil.SetProfil(subAccount);

            profil.TryGetComponent(out Button _btn);
            // Si le nombre max de profils est plus petit que le nombre de profil total, on desactive ces profils
            if (index >= myAccount.MaxSubAccounts)
                profil.LimitReached();

            // Cr�er une copie locale de l'index
            int currentIndex = index;

            // Ajout de l'�v�nement de clic
            _btn.onClick.AddListener(() => {
// isLoading A AJOUTER POUR EVITER UN DOUBLE CHARGEMENT DE PROFIL
                CanvasTransitionManager.Instance.PlayTransition(() => _canvasAuthManager.ActiveProfil(currentIndex));
            });
            profil.DeleteButton.onClick.AddListener(() => DeleteProfil(subAccount));

            index++;
        }
        newProfilGO?.SetAsLastSibling();
    }


    // Form New Profil UI
    public void AddNewProfil()
    {
        if (Validators.IsValidName(inputFieldNameProfil.text))
        {
            AuthManager.AddNewProfil(inputFieldNameProfil.text);
            _canvasAuthManager.ActiveProfil(myAccount.SubAccounts.Count - 1);
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
            BeforeActiveProfils();
            AfterGetProfils();
        }, "Attention !", "Voulez-vous vraiment supprimer le profil "+subAccount.Nom+" ? \nToutes les donn�es li�es � ce profil seront supprim�es et ne pourront plus �tre r�cup�r�es.");
    }
}