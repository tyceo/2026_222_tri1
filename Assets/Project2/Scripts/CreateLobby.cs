using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using System.Threading.Tasks;

//relay
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Collections.Generic;

public class CreateLobby : MonoBehaviour
{
    private Lobby currentLobby;

    public async void CreateLobbyFunction()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogError("not signed in yet");
            return;
        }

        string lobbyName = "My Lobby";
        int maxPlayers = 4;

        CreateLobbyOptions options = new CreateLobbyOptions
        {
            IsPrivate = false
        };

        currentLobby = await LobbyService.Instance.CreateLobbyAsync(
            lobbyName,
            maxPlayers,
            options
        );

        Debug.Log("Lobby created Code: " + currentLobby.LobbyCode);

        //relay and start host netcode

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
        transport.SetRelayServerData(allocation.ToRelayServerData("dtls"));

        NetworkManager.Singleton.StartHost();

        //store relay code in lobby
        var updateOptions = new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                {
                    "RelayCode",
                    new DataObject(DataObject.VisibilityOptions.Public, joinCode)
                }
            }
        };

        await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, updateOptions);

        Debug.Log("Relay started + Host running");

        StartCoroutine(HeartbeatLoop());
    }

    private System.Collections.IEnumerator HeartbeatLoop()
    {
        while (currentLobby != null)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
            yield return new WaitForSeconds(15f);
        }
    }
}