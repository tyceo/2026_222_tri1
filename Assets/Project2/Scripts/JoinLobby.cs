using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using TMPro;

//relay
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class JoinLobby : MonoBehaviour
{
    public TMP_InputField inputField; //inspector

    public async void JoinLobbyFromInput()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogError("not signed in yet");
            return;
        }

        string lobbyCode = inputField.text;

        Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

        Debug.Log("Joined lobby: " + lobby.Name);

        //relay and client

        string relayCode = lobby.Data["RelayCode"].Value;

        JoinAllocation joinAllocation =
            await RelayService.Instance.JoinAllocationAsync(relayCode);

        var transport =
            NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();

        transport.SetRelayServerData(joinAllocation.ToRelayServerData("dtls"));

        NetworkManager.Singleton.StartClient();

        Debug.Log("Connected to Relay + Client started");
    }
}