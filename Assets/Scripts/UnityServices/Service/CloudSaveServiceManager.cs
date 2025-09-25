using System;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using SaveOptions = Unity.Services.CloudSave.Models.Data.Player.SaveOptions;
using Unity.Services.CloudSave.Models.Data.Player;
using System.Collections.Generic;
using ServiceRequest;
using UnityEngine;

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

    public Request<Item> LoadPlayerData(string dataField, string playerID) => LoadPlayerData(dataField, true, playerID);

    public Request<Item> LoadPlayerData(string dataField, bool isPublicData) => LoadPlayerData(dataField, isPublicData, AuthenticationServiceManager.Instance.GetUserId());

    private Request<Item> LoadPlayerData(string dataField, bool isPublicData, string playerID)
    {
        Request<Item> request = new Request<Item>(async () =>
        {
            if (!AuthenticationServiceManager.Instance.IsSignedIn)
            {
                throw new Exception("You must be signed in to load cloud data.");
            }

            if (string.IsNullOrEmpty(dataField))
            {
                throw new Exception("Data key cannot be empty or null.");
            }

            HashSet<string> loadField = new HashSet<string>() { dataField };
            Dictionary<string, Item> loadData;

            if (isPublicData)
            {
                loadData = await CloudSaveService.Instance.Data.Player.LoadAsync(loadField, new LoadOptions(new PublicReadAccessClassOptions(playerID)));
            }
            else
            {
                loadData = await CloudSaveService.Instance.Data.Player.LoadAsync(loadField);
            }

            if (loadData.TryGetValue(dataField, out Item result))
            {
                return result;
            }
            else
            {
                throw new Exception("Failed to load data: the specified key was not found in the cloud save.");
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
                throw new Exception("You must be signed in to save cloud data.");
            }

            if (string.IsNullOrEmpty(dataField))
            {
                throw new Exception("Data key cannot be empty or null.");
            }

            if (saveItem == null)
            {
                throw new Exception("Cannot save null data.");
            }

            Dictionary<string, object> saveData = new Dictionary<string, object>() { { dataField, saveItem } };
            if (isPublicData)
            {
                await CloudSaveService.Instance.Data.Player.SaveAsync(saveData, new SaveOptions(new PublicWriteAccessClassOptions()));
                Debug.Log($"Save public data: {dataField}");
            }
            else
            {
                await CloudSaveService.Instance.Data.Player.SaveAsync(saveData);
                Debug.Log($"Save private data: {dataField}");
            }
        });

        return request;
    }

    public void Dispose()
    {
        instance = null;
        isDisposed = true;
    }
}