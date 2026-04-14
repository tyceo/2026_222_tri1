using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using System.Threading.Tasks;

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

        //start heartbeat loop
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