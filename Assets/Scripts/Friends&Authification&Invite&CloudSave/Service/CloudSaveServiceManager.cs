using System;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using SaveOptions = Unity.Services.CloudSave.Models.Data.Player.SaveOptions;
using UnityEngine;
using Unity.Services.CloudSave.Models.Data.Player;
using System.Collections.Generic;

public class CloudSaveServiceManager : IDisposable
{
    private static bool isDisposed = false;
    private static CloudSaveServiceManager instance;
    public static CloudSaveServiceManager Instance 
    { 
        get
        {
            if (instance == null && !isDisposed)
            {
                instance = new CloudSaveServiceManager();
            }

            return instance;
        }
    }

    private CloudSaveServiceManager() { }

    public bool LoadPlayerData(string dataField, Action<bool, Item> callback = null, string playerID = null)
    {
        if (!AuthenticationServiceManager.Instance.IsSignedIn || string.IsNullOrEmpty(dataField)) { return false; }

        _ = LoadPlayerDataAsync(dataField, callback, playerID);

        return true;
    }

    public bool SavePlayerData(string dataField, object saveItem, bool isPublicData)
    {
        if (!AuthenticationServiceManager.Instance.IsSignedIn || string.IsNullOrEmpty(dataField) || saveItem == null) { return false; }

        _ = SavePlayerDataAsync(dataField, saveItem, isPublicData);

        return true;
    }

    private async Task LoadPlayerDataAsync(string dataField, Action<bool, Item> callback = null, string playerID = null)
    {
        try
        {
            Dictionary<string, Item> loadData = new Dictionary<string, Item>();

            LoadOptions loadOptions = null;

            if (string.IsNullOrEmpty(playerID))
            {
                loadOptions = new LoadOptions(new PublicReadAccessClassOptions(AuthenticationServiceManager.Instance.GetUserId()));
            }
            else
            {
                loadOptions = new LoadOptions(new PublicReadAccessClassOptions(playerID));
            }

            loadData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>() { dataField }, loadOptions);

            if (loadData.TryGetValue(dataField, out Item result))
            {
                callback?.Invoke(true, result);
                return;
            }

            callback?.Invoke(false, null);
        }
        catch (CloudSaveException e)
        {
            Debug.Log(e);

            callback?.Invoke(false, null);
        }
    }

    private async Task SavePlayerDataAsync(string dataField, object saveItem, bool isPublicData)
    {
        SaveOptions saveOptions;
        if (isPublicData)
        {
            saveOptions = new SaveOptions(new PublicWriteAccessClassOptions());
        }
        else
        {
            saveOptions = new SaveOptions();
        }

        Dictionary<string, object> saveData = new Dictionary<string, object>() { { dataField, saveItem } };

        try
        {
            await CloudSaveService.Instance.Data.Player.SaveAsync(saveData, saveOptions);

            Debug.Log($"[CloudSave] Data saved: {dataField}");
        }
        catch (CloudSaveException e)
        {
            Debug.Log(e);
        }
    }

    public void Dispose()
    {
        instance = null;
        isDisposed = true;
    }
}