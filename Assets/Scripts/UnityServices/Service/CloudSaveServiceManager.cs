using System;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using SaveOptions = Unity.Services.CloudSave.Models.Data.Player.SaveOptions;
using Unity.Services.CloudSave.Models.Data.Player;
using System.Collections.Generic;
using ServiceRequest;

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

    public Request<Item> LoadPlayerData(string dataField, string playerID = null)
    {
        Request<Item> request = new Request<Item>(async () => 
        {
            if (!AuthenticationServiceManager.Instance.IsSignedIn)
            {
                throw new Exception("Please sign in");
            }

            if (string.IsNullOrEmpty(dataField))
            {
                throw new Exception("Invalid data key");
            }

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
                return result;
            }
            else
            {
                throw new Exception("Failed load data, data not found");
            }

        });

        return request;
    }

    public Request SavePlayerData(string dataField, object saveItem, bool isPublicData)
    {
        Request request = new Request(async () =>
        {
            if (!AuthenticationServiceManager.Instance.IsSignedIn)
            {
                throw new Exception("Please sign in");
            }

            if (string.IsNullOrEmpty(dataField))
            {
                throw new Exception("Invalid data key");
            }

            if (saveItem == null)
            {
                throw new Exception("Invalid save item");
            }

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

            await CloudSaveService.Instance.Data.Player.SaveAsync(saveData, saveOptions);
        });

        return request;
    }

    public void Dispose()
    {
        instance = null;
        isDisposed = true;
    }
}