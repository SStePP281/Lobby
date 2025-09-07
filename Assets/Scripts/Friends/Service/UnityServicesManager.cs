using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;

public class UnityServicesManager : IDisposable
{
    private static bool isDisposing = false;
    private static UnityServicesManager instance;
    public static UnityServicesManager Instance
    {
        get
        {
            if (isDisposing) { return null; }

            if (instance == null)
            {
                instance = new UnityServicesManager();
            }    

            return instance;
        }
    }

    private UnityServicesManager() {}

    public async Task Initialize()
    {
        if (IsInitialized()) { return; }

        await UnityServices.InitializeAsync();

        Debug.Log("UnityServices Initialized");
    }

    public bool IsInitialized() => UnityServices.Instance.State == ServicesInitializationState.Initialized;

    public void Dispose()
    {
        instance = null;

        isDisposing = true;
    }   
}
