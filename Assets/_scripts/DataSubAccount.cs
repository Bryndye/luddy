using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using UnityEngine;

public enum LevelStateType
{
    isUnlocked, isCompleted, isLocked
}

[Serializable]
class DataLevel
{
    public string Name;
    public int Level;
    public LevelStateType MyLevelState;
}

public class DataSubAccount
{
    public SubAccount CurrentSubAccount;
    private string keyCloud { get { return CurrentSubAccount.Id; } }

    #region Controllers
    public async Task<Dictionary<string, object>> LoadAllDatas(bool returnData = false)
    {
        try
        {
            var allDatas = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { keyCloud });

            allDatas.TryGetValue(keyCloud, out var _accountDb);

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
            DataSubAccount _dataPlayer = JsonUtility.FromJson<DataSubAccount>(value);


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
            await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> { { keyCloud, this } });
        }
        catch (Exception e)
        {
            Debug.LogError("Erreur lors de la sauvegarde des données : " + e.Message);
        }
    }
    #endregion
}
