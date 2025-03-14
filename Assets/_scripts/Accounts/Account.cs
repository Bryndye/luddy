using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using UnityEngine;

[Serializable]
public class SubAccount
{
    public string Id;
    public string Nom;
    public List<LevelDatasPlayer> MyLevelDatasPlayer;

    public SubAccount(string id, string nom)
    {
        Id = id;
        Nom = nom;
        MyLevelDatasPlayer = new List<LevelDatasPlayer>();
    }

    public void AddLevelDataPlayer(LevelDatasPlayer levelDatasPlayer)
    {
        if (MyLevelDatasPlayer.Any(x => x.LevelId == levelDatasPlayer.LevelId))
        {
            MyLevelDatasPlayer.Remove(MyLevelDatasPlayer.Find(x => x.LevelId == levelDatasPlayer.LevelId));
        }
        MyLevelDatasPlayer.Add(levelDatasPlayer);
        Debug.Log("Ajout level datas");
    }
}

[Serializable]
public class Account
{
    private string id;
    public string Id { get { return id; } }

    public List<SubAccount> SubAccounts = new List<SubAccount>();
    public int MaxSubAccounts = 2;

    public bool isSubscribed = false;
    public DateTime timeToSubscribed;
    public DateTime timeEndSubscribed;

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
            // Charge toutes les donn�es disponibles pour l'utilisateur
            var allDatas = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "Account" });
            return allDatas.Count >= 0;
        }
        catch (Exception e)
        {
            Debug.LogError("Erreur lors de la v�rification des donn�es : " + e.Message);
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

            // Si pas de donn�es, retourner null
            if (_accountDb == null)
            {
                return null;
            }

            // R�cup�ration des donn�es puis set
            var value = JsonConvert.SerializeObject(_accountDb.Value);
            Account _account = JsonUtility.FromJson<Account>(value);

            SubAccounts = _account.SubAccounts;
            MaxSubAccounts = _account.MaxSubAccounts;
            isSubscribed = _account.isSubscribed;
            timeToSubscribed = _account.timeToSubscribed;
            timeEndSubscribed = _account.timeEndSubscribed;

            VerifySubscription();

            return null;

        }
        catch (Exception e)
        {
            Debug.LogError("Erreur lors de la r�cup�ration des donn�es : " + e.Message);
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
            Debug.LogError("Erreur lors de la r�cup�ration des donn�es : " + e.Message);
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
            Debug.LogError("Erreur lors de la sauvegarde des donn�es : " + e.Message);
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

        // Si pas de nom, nom par d�faut
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
        if (SubAccounts.Contains(subAccount))
        {
            SubAccounts.Remove(subAccount);
            SetDatas();
        }
        else
        {
            // Si l'objet n'est pas trouv�, on le recherche par son id
            foreach (var item in SubAccounts)
            {
                if (item.Id == subAccount.Id)
                {
                    SubAccounts.Remove(item);
                    SetDatas();
                    return;
                }
            }
        }
    }

    public SubAccount GetProfil(int index)
    {
        return SubAccounts[index];
    }


    public void SetSubscription(bool subscribed)
    {
        isSubscribed = subscribed;
        string state = subscribed ? "on" : "off";
        Debug.Log("L'�tat de votre abonnement est : " + state);
        if (subscribed) { 
            timeToSubscribed = DateTime.Now;
            timeEndSubscribed = timeToSubscribed;
            timeEndSubscribed.AddYears(1);
            MaxSubAccounts = 4;
        }
        else
        {
            timeToSubscribed = DateTime.MinValue;
            timeEndSubscribed = timeToSubscribed;
            MaxSubAccounts = 1;
        }
        SetDatas();
    }

    private void VerifySubscription()
    {
        if (!isSubscribed) return;

        if (timeToSubscribed > timeEndSubscribed)
        {
            SetSubscription(false);
        }
    }
    #endregion
}