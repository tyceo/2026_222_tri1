using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;

public class CreateLobby : MonoBehaviour
{
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

        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(
            lobbyName,
            maxPlayers,
            options
        );

        Debug.Log("Lobby created Code: " + lobby.LobbyCode);
    }
}