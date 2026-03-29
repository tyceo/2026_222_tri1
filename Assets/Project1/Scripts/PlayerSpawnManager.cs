using UnityEngine;
using Unity.Netcode;

public class PlayerSpawnManager : NetworkBehaviour
{
    [Header("Spawn Positions")]
    [SerializeField] private Transform hostSpawnPoint;
    [SerializeField] private Transform clientSpawnPoint;
    
    [Header("Default Positions (if no spawn points assigned)")]
    [SerializeField] private Vector3 hostSpawnPosition = new Vector3(-5f, 1f, 0f);
    [SerializeField] private Vector3 clientSpawnPosition = new Vector3(5f, 1f, 0f);

    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        }
    }

    private void OnServerStarted()
    {
        //register connection approval callback
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        
        PositionHostPlayer();
    }

    private void PositionHostPlayer()
    {
        //manually position host since they spawn before callback is registered
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(0, out var hostClient))
        {
            if (hostClient.PlayerObject != null)
            {
                Vector3 hostSpawn = hostSpawnPoint != null ? hostSpawnPoint.position : hostSpawnPosition;
                hostClient.PlayerObject.transform.position = hostSpawn;
                
                Model model = hostClient.PlayerObject.GetComponent<Model>();
                if (model != null)
                {
                    model.ForceSetPosition(hostSpawn);
                }
                
            }
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        //server approves connection and sets spawn position
        response.Approved = true;
        response.CreatePlayerObject = true;

        Vector3 spawnPosition = GetSpawnPosition(request.ClientNetworkId);
        response.Position = spawnPosition;
        response.Rotation = Quaternion.identity;
    }

    private Vector3 GetSpawnPosition(ulong clientId)
    {
        if (clientId == 0)
        {
            return hostSpawnPoint != null ? hostSpawnPoint.position : hostSpawnPosition;
        }
        else
        {
            return clientSpawnPoint != null ? clientSpawnPoint.position : clientSpawnPosition;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }
    }
}