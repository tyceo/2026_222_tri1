using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class Model : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    [SerializeField] private float interpolationSpeed = 15f;

    //syncs position across all clients
    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    
    //syncs player name across network
    private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    
    //syncs hat selection across network
    private NetworkVariable<int> hatIndex = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    
    private Vector2 currentInput;
    private View view;
    private FirstPersonCamera fpsCamera;

    void Start()
    {
        view = GetComponent<View>();
        fpsCamera = GetComponent<FirstPersonCamera>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        //server sets initial position
        if (IsServer)
        {
            networkPosition.Value = transform.position;
        }
        
        //client requests their chosen name be set
        if (IsOwner && !IsServer)
        {
            string chosenName = PlayerNameManager.Instance != null 
                ? PlayerNameManager.Instance.PlayerName 
                : $"Player{OwnerClientId}";
            
            int chosenHat = PlayerHatManager.Instance != null
                ? PlayerHatManager.Instance.SelectedHatIndex
                : 0;
            
            RequestSetPlayerDataServerRpc(chosenName, chosenHat);
        }
        
        //server (host) sets their own name and hat
        if (IsServer && IsOwner)
        {
            string chosenName = PlayerNameManager.Instance != null 
                ? PlayerNameManager.Instance.PlayerName 
                : "Host";
            
            int chosenHat = PlayerHatManager.Instance != null
                ? PlayerHatManager.Instance.SelectedHatIndex
                : 0;
            
            playerName.Value = chosenName;
            hatIndex.Value = chosenHat;
            Debug.Log($"Server set own name: {playerName.Value}, hat: {hatIndex.Value}");
        }
        
        //subscribe to network variable changes
        playerName.OnValueChanged += OnPlayerNameChanged;
        hatIndex.OnValueChanged += OnHatChanged;
        
        Invoke(nameof(InitializeNametag), 0.1f);
        Invoke(nameof(InitializeHat), 0.1f);
    }

    [ServerRpc]
    private void RequestSetPlayerDataServerRpc(string name, int hat, ServerRpcParams rpcParams = default)
    {
        // Validate name length
        if (name.Length > 12)
        {
            name = name.Substring(0, 12);
        }
        
        if (string.IsNullOrEmpty(name))
        {
            name = $"Player{OwnerClientId}";
        }
        
        // Validate hat index
        if (hat < 0 || hat > 1)
        {
            hat = 0;
        }
        
        playerName.Value = name;
        hatIndex.Value = hat;
        Debug.Log($"Server set name for ClientID {OwnerClientId}: {playerName.Value}, hat: {hatIndex.Value}");
    }

    private void InitializeNametag()
    {
        if (view != null)
        {
            Debug.Log($"Setting nametag to: {playerName.Value}");
            view.SetPlayerName(playerName.Value.ToString());
        }
    }

    private void InitializeHat()
    {
        if (view != null)
        {
            Debug.Log($"Setting hat to: {hatIndex.Value}");
            view.SetHat(hatIndex.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        //unsubscribe from network variable callbacks
        playerName.OnValueChanged -= OnPlayerNameChanged;
        hatIndex.OnValueChanged -= OnHatChanged;
    }

    private void OnPlayerNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        if (view != null)
        {
            Debug.Log($"Name changed from {previousValue} to {newValue}");
            view.SetPlayerName(newValue.ToString());
        }
    }

    private void OnHatChanged(int previousValue, int newValue)
    {
        if (view != null)
        {
            Debug.Log($"Hat changed from {previousValue} to {newValue}");
            view.SetHat(newValue);
        }
    }

    void Update()
    {
        //server processes movement and updates network position
        if (IsServer)
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            
            Vector3 movement = (right * currentInput.x + forward * currentInput.y) * moveSpeed * Time.deltaTime;
            transform.position += movement;
            
            networkPosition.Value = transform.position;
            
            currentInput = Vector2.zero;
        }
        
        //clients interpolate to server's authoritative position
        if (!IsServer)
        {
            transform.position = Vector3.Lerp(transform.position, networkPosition.Value, interpolationSpeed * Time.deltaTime);
        }

        if (view != null)
        {
            view.UpdateVisuals(transform.position);
        }
    }

    public void SetMovementInput(Vector2 input)
    {
        //only server processes movement input
        if (IsServer)
        {
            currentInput = input.normalized;
        }
    }

    //forces position update from spawn manager
    public void ForceSetPosition(Vector3 position)
    {
        if (IsServer)
        {
            transform.position = position;
            networkPosition.Value = position;
        }
    }

    //get player name for UI display
    public string GetPlayerName()
    {
        return playerName.Value.ToString();
    }
}
