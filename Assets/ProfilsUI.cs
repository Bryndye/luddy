using System;
using System.Collections;
using UnityEngine;

public class ProfilsUI : MonoBehaviour
{
    [SerializeField] private Account Account;
    [SerializeField] private Transform container;
    [SerializeField] private ProfilUI profilUIPrefab;


    public void ActiveProfilsUIAccount(Account account)
    {
        gameObject.SetActive(true);
        StartCoroutine(ActivateAccountWithDelay(account, 1f)); // Délai d'attente de 1 seconde
    }

    private IEnumerator ActivateAccountWithDelay(Account account, float delay)
    {
        Account = account;

        // Forcer un délai avant d'activer le profil
        yield return new WaitForSeconds(delay);

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
                            Debug.Log("New Profil");
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

        // Sauf si le nombre max de subAccount est atteint
        if (Account.MyPlayerSubAccounts.SubAccounts.Count >= Account.MaxSubAccounts)
        {
            // Pas de nouveau profil disponible
            Destroy(newProfilT.gameObject);
        }


        foreach (SubAccount subAccount in Account.MyPlayerSubAccounts.SubAccounts)
        {
            Instantiate(profilUIPrefab, container).SetProfil(subAccount);
        }
        newProfilT?.SetAsLastSibling();
    }

}