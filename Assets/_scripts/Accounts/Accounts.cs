using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.CloudSave;
using System.Threading.Tasks;
using Unity.Services.Authentication;

[Serializable]
public class SubAccount
{
    public string Id;
    public string Nom;

    public SubAccount(string id, string nom)
    {
        Id = id;
        Nom = nom;
    }
}

[Serializable]
public class Account
{
    private string id;
    public string Id { get { return id; } }

    public List<SubAccount> SubAccounts = new List<SubAccount>();
    public int MaxSubAccounts = 2;
    public Dictionary<string, Unity.Services.CloudSave.Models.Item> Datas;

    public Account(string _id)
    {
        id = _id;
        Debug.Log("Account : " + id);
        InitData();
    }

    private async void InitData()
    {
        // Si le joueur a des datas, ne pas re-init
        if (await CheckIfUserDataExists())
        {
            return;
        }

        // Init du premier subaccount
        string newId = id + 1.ToString();
        SubAccount subAccount = new SubAccount(newId, "custom name ");
        List<SubAccount> subAccounts = new List<SubAccount>
        {
            subAccount
        };

        // init de la structure de datas
        try
        {
            var iniData = new Dictionary<string, object>
            {
                { "SubAccounts", subAccounts},
                { "MaxSubAccounts", 2},
            };
            await CloudSaveService.Instance.Data.Player.SaveAsync(iniData);

            // TEST
            await ChargerCompte();
        }
        catch (Exception e)
        {
            Debug.LogError("Erreur lors de la sauvegarde des donn�es : " + e.Message);
        }
    }

    private async Task<bool> CheckIfUserDataExists()
    {
        try
        {
            // Charge toutes les donn�es disponibles pour l'utilisateur
            var allData = await CloudSaveService.Instance.Data.Player.LoadAllAsync();

            // V�rifie si des donn�es existent
            return allData.Count > 0;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur lors de la v�rification des donn�es : " + e.Message);
            return false;
        }
    }

    // Sauvegarde des donn�es dans le Cloud
    //public async Task SauvegarderCompte()
    //{
    //    try
    //    {
    //        var data = new Dictionary<string, object>
    //        {
    //            { "ComptePrincipal_" + Id, JsonUtility.ToJson(this) }
    //        };
    //        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    //        Debug.Log("Donn�es sauvegard�es avec succ�s.");
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogError("Erreur de sauvegarde: " + ex.Message);
    //    }
    //}

    // Chargement des donn�es depuis le Cloud
    public async Task ChargerCompte()
    {
        try
        {
            Datas = await CloudSaveService.Instance.Data.Player.LoadAllAsync();
            Debug.Log(Datas);
            //if (result.TryGetValue("ComptePrincipal_" + Id, out var jsonData))
            //{
            //    JsonUtility.FromJsonOverwrite(jsonData, this);
            //    Debug.Log("Donn�es charg�es avec succ�s.");
            //}
            //else
            //{
            //    Debug.Log("Aucune donn�e trouv�e pour ce compte.");
            //}
        }
        catch (Exception ex)
        {
            Debug.LogError("Erreur de chargement: " + ex.Message);
        }
    }
}
