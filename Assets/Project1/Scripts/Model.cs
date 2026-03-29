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
        
        //server assigns name based on client id
        if (IsServer)
        {
            if (OwnerClientId == 0)
            {
                playerName.Value = "Player1";
            }
            else
            {
                playerName.Value = "Player2";
            }
            
            Debug.Log($"Server set name for ClientID {OwnerClientId}: {playerName.Value}");
        }
        
        //subscribe to network variable changes
        playerName.OnValueChanged += OnPlayerNameChanged;
        
        Invoke(nameof(InitializeNametag), 0.1f);
    }

    private void InitializeNametag()
    {
        if (view != null)
        {
            Debug.Log($"Setting nametag to: {playerName.Value}");
            view.SetPlayerName(playerName.Value.ToString());
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        //unsubscribe from network variable callbacks
        playerName.OnValueChanged -= OnPlayerNameChanged;
    }

    private void OnPlayerNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        if (view != null)
        {
            Debug.Log($"Name changed from {previousValue} to {newValue}");
            view.SetPlayerName(newValue.ToString());
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
}
