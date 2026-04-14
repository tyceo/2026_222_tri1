using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using TMPro;

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
    }
}