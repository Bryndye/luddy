using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

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
public class PlayerSubAccounts
{
    public List<SubAccount> SubAccounts = new List<SubAccount>();
}

[Serializable]
public class Account
{
    private string id;
    public string Id { get { return id; } }

    public PlayerSubAccounts MyPlayerSubAccounts;
    public int MaxSubAccounts = 0;
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
            LoadAllDatas();
            return;
        }

        // Init du premier subaccount
        string newId = id + 1.ToString();
        SubAccount subAccount = new SubAccount(newId, "custom name");
        PlayerSubAccounts _tempPSA = new PlayerSubAccounts();
        _tempPSA.SubAccounts.Add(subAccount);

        // init de la structure de datas
        try
        {
            var iniData = new Dictionary<string, object>
            {
                { "SubAccounts", _tempPSA},
                { "MaxSubAccounts", 2},
            };
            await CloudSaveService.Instance.Data.Player.SaveAsync(iniData);

            LoadAllDatas();
        }
        catch (Exception e)
        {
            Debug.LogError("Erreur lors de la sauvegarde des donn�es : " + e.Message);
        }
    }

    #region middleware
    private async Task<bool> CheckIfUserDataExists()
    {
        try
        {
            // Charge toutes les donn�es disponibles pour l'utilisateur
            var allData = await CloudSaveService.Instance.Data.Player.LoadAllAsync();

            // V�rifie si des donn�es existent
            return allData.Count > 0;
        }
        catch (Exception e)
        {
            Debug.LogError("Erreur lors de la v�rification des donn�es : " + e.Message);
            return false;
        }
    }
    #endregion

    #region Controllers
    public async void LoadAllDatas()
    {
        Datas = await CloudSaveService.Instance.Data.Player.LoadAllAsync();

        Datas.TryGetValue("SubAccounts", out var _subAccounts);
        var value = JsonConvert.SerializeObject(_subAccounts.Value);
        MyPlayerSubAccounts = JsonUtility.FromJson<PlayerSubAccounts>(value);
        Debug.Log("subaccounts done");

        Datas.TryGetValue("MaxSubAccounts", out var _maxSubAccounts);
        value = JsonConvert.SerializeObject(_maxSubAccounts.Value);
        MaxSubAccounts = int.Parse(value);

        //Datas.TryGetValue("MaxSubAccounts", out var _maxSubAccounts);
        try
        {

            // Parcourir toutes les cl�s et valeurs
            //foreach (var kvp in Datas)
            //{
            //    string key = kvp.Key;
            //    string value = kvp.Value.ToString(); // Convertir en string pour traitement

            //    Debug.Log($"Key: {key}, Value: {value}");

            //    // Exemple : D�s�rialiser une valeur sp�cifique si n�cessaire
            //    if (key == "SubAccounts")
            //    {
            //        value = JsonConvert.SerializeObject(item.Value);
            //        MyPlayerSubAccounts = JsonUtility.FromJson<PlayerSubAccounts>(value);
            //    }
            //    else if (key == "MaxSubAccounts")
            //    {
            //        MaxSubAccounts = int.Parse(value);
            //    }
            //}
        }
        catch (Exception e)
        {
            Debug.LogError("Erreur lors du chargement des donn�es : " + e.Message);
        }
    }

    private async void SetCloudDataToObject()
    {
        var retrievedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "SubAccounts" });

        if (retrievedData.TryGetValue("SubAccounts", out var item))
        {
            try
            {
                var test = JsonConvert.SerializeObject(item.Value);

                MyPlayerSubAccounts = JsonUtility.FromJson<PlayerSubAccounts>(test);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Deserialization failed: {ex.Message}");
            }
        }
    }

    #endregion

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
