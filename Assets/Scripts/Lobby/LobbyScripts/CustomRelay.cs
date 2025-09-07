using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class CustomRelay
{
    public static CustomRelay Instance { get; private set; }

    private UnityTransport unityTransport;

    public CustomRelay()
    {
        if (Instance == null)
        {
            Instance = this;

            unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        }
    }

    public async Task AuthenticateClient()
    {
        await UnityServices.InitializeAsync();

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Debug.Log("Singed in " + AuthenticationService.Instance.PlayerId);
    }

    public async Task CreateRelay()
    {
        try
        {
            //Allocation allocation = await RelayService.Instance.CreateAllocationAsync(LobbyManager.Instance.GetLobbyDataSO().maxLobbySize);
            //
            //string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            //
            //RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            //
            //unityTransport.SetRelayServerData(relayServerData);
            //
            //NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async Task JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining relay with " + joinCode);

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            unityTransport.SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
}
