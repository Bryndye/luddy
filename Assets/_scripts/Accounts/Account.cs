using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.Rendering;

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

    public Account(string _id)
    {
        id = _id;

        InitData();
    }

    private async void InitData()
    {
        // Si le joueur a des datas, ne pas re-init
        if (await CheckIfUserDataExists())
        {
            await LoadAllDatas();
            return;
        }
    }

    #region middleware
    private async Task<bool> CheckIfUserDataExists()
    {
        try
        {
            // Charge toutes les données disponibles pour l'utilisateur
            var allDatas = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "Account" });
            return allDatas.Count >= 0;
        }
        catch (Exception e)
        {
            Debug.LogError("Erreur lors de la vérification des données : " + e.Message);
            return false;
        }
    }
    #endregion

    #region Controllers
    public async Task<Dictionary<string, object>> LoadAllDatas(bool returnData = false)
    {
        try
        {
            var allDatas = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "Account" });

            allDatas.TryGetValue("Account", out var _accountDb);

            if (returnData)
            {
                var json = JsonConvert.SerializeObject(_accountDb.Value);
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            }

            // Si pas de données, retourner null
            if (_accountDb == null)
            {
                return null;
            }

            // Récupération des données puis set
            var value = JsonConvert.SerializeObject(_accountDb.Value);
            Account _account = JsonUtility.FromJson<Account>(value);

            SubAccounts = _account.SubAccounts;
            MaxSubAccounts = _account.MaxSubAccounts;

            return null;

        }
        catch (Exception e)
        {
            Debug.LogError("Erreur lors de la récupération des données : " + e.Message);
            return null;
        }
    }

    public async Task<object> LoadData(string key)
    {
        try
        {
            var data = await LoadAllDatas(true);
            data.TryGetValue(key, out var _data);
            return _data;
        }
        catch (Exception e)
        {
            Debug.LogError("Erreur lors de la récupération des données : " + e.Message);
            return null;
        }
    }

    public async void SetDatas()
    {
        try
        {
            await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> { { "Account", this } });
        }
        catch (Exception e)
        {
            Debug.LogError("Erreur lors de la sauvegarde des données : " + e.Message);
        }
    }

    public void AddNewProfil(string name = null)
    {
        if (SubAccounts.Count >= MaxSubAccounts)
        {
            Debug.Log("Nombre maximum de profils atteint");
            return;
        }

        string newId = id + (SubAccounts.Count + 1).ToString();

        // Si pas de nom, nom par défaut
        if (name == null)
        {
            name = "Profil" + (SubAccounts.Count + 1).ToString();
        }

        SubAccount subAccount = new SubAccount(newId, name);
        SubAccounts.Add(subAccount);

        SetDatas();
    }

    public void DeleteProfil(int index)
    {
        SubAccounts.RemoveAt(index);
        SetDatas();
    }

    public void DeleteProfil(SubAccount subAccount)
    {
        SubAccounts.Remove(subAccount);
        SetDatas();
    }

    public SubAccount GetProfil(int index)
    {
        return SubAccounts[index];
    }
    #endregion
}