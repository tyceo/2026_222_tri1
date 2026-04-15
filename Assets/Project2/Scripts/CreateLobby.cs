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
using TMPro;

public class CreateLobby : MonoBehaviour
{
    private Lobby currentLobby;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI lobbyCodeText;
    
    [SerializeField] private GameObject hideWhenLobbyCreated;
    [SerializeField] private GameObject hideWhenLobbyCreated2;
    [SerializeField] private GameObject hideWhenLobbyCreated3;
    [SerializeField] private GameObject hideWhenLobbyCreated4;

    public async void CreateLobbyFunction()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogError("not signed in yet");
            return;
        }

        string lobbyName = PlayerNameManager.Instance != null 
            ? PlayerNameManager.Instance.PlayerName + "'s Lobby"
            : "My Lobby";
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
        
        //display lobby code on UI and go to game 
        if (lobbyCodeText != null)
        {
            lobbyCodeText.text = $"Lobby Code: {currentLobby.LobbyCode}";
            hideWhenLobbyCreated.SetActive(false);
            hideWhenLobbyCreated2.SetActive(false);
            hideWhenLobbyCreated3.SetActive(false);
            hideWhenLobbyCreated4.SetActive(false);
        }

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

    public async void CreatePrivateLobbyFunction()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogError("not signed in yet");
            return;
        }

        string lobbyName = PlayerNameManager.Instance != null 
            ? PlayerNameManager.Instance.PlayerName + "'s Lobby"
            : "My Private Lobby";
        int maxPlayers = 4;

        CreateLobbyOptions options = new CreateLobbyOptions
        {
            IsPrivate = true
        };

        currentLobby = await LobbyService.Instance.CreateLobbyAsync(
            lobbyName,
            maxPlayers,
            options
        );

        Debug.Log("Private Lobby created Code: " + currentLobby.LobbyCode);
        
        //display lobby code on UI and go to game 
        if (lobbyCodeText != null)
        {
            lobbyCodeText.text = $"Lobby Code: {currentLobby.LobbyCode}";
            hideWhenLobbyCreated.SetActive(false);
            hideWhenLobbyCreated2.SetActive(false);
            hideWhenLobbyCreated3.SetActive(false);
            hideWhenLobbyCreated4.SetActive(false);
        }

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