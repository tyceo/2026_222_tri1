using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using System.Collections.Generic;
using TMPro;

//relay
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class LobbyBrowser : MonoBehaviour
{
    [Header("Lobby List UI")]
    [SerializeField] private Transform lobbyListContainer;
    [SerializeField] private GameObject lobbyListItemPrefab;
    [SerializeField] private TextMeshProUGUI statusText;
    
    [Header("UI to Hide When Joining")]
    [SerializeField] private GameObject[] hideWhenJoining;
    
    private List<GameObject> spawnedListItems = new List<GameObject>();
    private float refreshTimer = 0f;
    private float refreshInterval = 3f; //refresh every 3 seconds

    void Update()
    {
        //auto-refresh lobby list
        refreshTimer += Time.deltaTime;
        if (refreshTimer >= refreshInterval)
        {
            refreshTimer = 0f;
            RefreshLobbyList();
        }
    }

    public async void RefreshLobbyList()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            if (statusText != null)
                statusText.text = "Not signed in";
            return;
        }

        try
        {
            //query for public lobbies
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Count = 25, //max number of lobbies to retrieve
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                }
            };

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(options);

            //clear existing list items
            foreach (GameObject item in spawnedListItems)
            {
                Destroy(item);
            }
            spawnedListItems.Clear();

            //create new list items
            foreach (Lobby lobby in queryResponse.Results)
            {
                GameObject listItem = Instantiate(lobbyListItemPrefab, lobbyListContainer);
                LobbyListItem itemScript = listItem.GetComponent<LobbyListItem>();
                
                if (itemScript != null)
                {
                    itemScript.Setup(
                        lobby.Id,
                        lobby.Name,
                        lobby.Players.Count,
                        lobby.MaxPlayers,
                        this
                    );
                }
                
                spawnedListItems.Add(listItem);
            }

            if (statusText != null)
            {
                if (queryResponse.Results.Count == 0)
                {
                    statusText.text = "No lobbies available";
                }
                else
                {
                    statusText.text = $"{queryResponse.Results.Count} lobbies found";
                }
            }

            //Debug.Log($"Found {queryResponse.Results.Count} lobbies");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to query lobbies: {e}");
            if (statusText != null)
                statusText.text = "Failed to load lobbies";
        }
    }

    public async void JoinLobbyById(string lobbyId)
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogError("Not signed in yet");
            return;
        }

        try
        {
            if (statusText != null)
                statusText.text = "Joining lobby...";

            //hide UI elements
            foreach (GameObject obj in hideWhenJoining)
            {
                if (obj != null)
                    obj.SetActive(false);
            }

            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

            Debug.Log("Joined lobby: " + lobby.Name);

            //connect to relay
            string relayCode = lobby.Data["RelayCode"].Value;

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(joinAllocation.ToRelayServerData("dtls"));

            NetworkManager.Singleton.StartClient();

            Debug.Log("Connected to Relay + Client started");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to join lobby: {e}");
            if (statusText != null)
                statusText.text = "Failed to join lobby";
            
            //re-show UI elements
            foreach (GameObject obj in hideWhenJoining)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }
    }

    void OnEnable()
    {
        //refresh when panel is shown
        RefreshLobbyList();
    }
}