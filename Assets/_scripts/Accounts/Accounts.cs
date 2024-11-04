using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TreeEditor;
using Unity.Services.CloudSave;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;

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
            SetCloudDataToObject();
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
                { "SubAccounts", JsonUtility.ToJson(_tempPSA)},
                { "MaxSubAccounts", 2},
            };
            await CloudSaveService.Instance.Data.Player.SaveAsync(iniData);

            SetCloudDataToObject();

            // TEST
            await ChargerCompte();
        }
        catch (Exception e)
        {
            Debug.LogError("Erreur lors de la sauvegarde des données : " + e.Message);
        }
    }

    private async Task<bool> CheckIfUserDataExists()
    {
        try
        {
            // Charge toutes les données disponibles pour l'utilisateur
            var allData = await CloudSaveService.Instance.Data.Player.LoadAllAsync();

            // Vérifie si des données existent
            return allData.Count > 0;
        }
        catch (Exception e)
        {
            Debug.LogError("Erreur lors de la vérification des données : " + e.Message);
            return false;
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
                test = test.Replace("\\\"", "\"").Replace("\\", "");
                if (test.StartsWith("\"") && test.EndsWith("\""))
                {
                    test = test[1..^1]; // Enlève le premier et le dernier caractère
                }
                Debug.Log(test);
                //string bite = "{\"SubAccounts\":[{\"Id\":\"xdixwR2C1FHoydJTqfLTFNq5Ha5k1\",\"Nom\":\"custom name\"}]}";
                MyPlayerSubAccounts = JsonUtility.FromJson<PlayerSubAccounts>(test);
                //PlayerSubAccounts playerSubAccounts = JsonConvert.DeserializeObject<PlayerSubAccounts>(test);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Deserialization failed: {ex.Message}");
            }
        }
    }



    // Chargement des données depuis le Cloud
    public async Task ChargerCompte()
    {
        try
        {
            Datas = await CloudSaveService.Instance.Data.Player.LoadAllAsync();
            Debug.Log(Datas);
            //if (result.TryGetValue("ComptePrincipal_" + Id, out var jsonData))
            //{
            //    JsonUtility.FromJsonOverwrite(jsonData, this);
            //    Debug.Log("Données chargées avec succès.");
            //}
            //else
            //{
            //    Debug.Log("Aucune donnée trouvée pour ce compte.");
            //}
        }
        catch (Exception ex)
        {
            Debug.LogError("Erreur de chargement: " + ex.Message);
        }
    }
}
