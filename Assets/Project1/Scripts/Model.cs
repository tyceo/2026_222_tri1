using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class Model : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    //networked position
    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
    
    //networked player name using FixedString (better for Netcode)
    private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    
    private Vector2 currentInput;
    private View view;

    void Start()
    {
        view = GetComponent<View>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        //set player name on spawn
        if (IsServer)
        {
            //check the owner's client ID
            //host's client ID is 0 others higher are 
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
        
        //listen for name changes
        playerName.OnValueChanged += OnPlayerNameChanged;
        
        //initialize nametag for all clients and delay slightly 
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
        
        //unsubscribe from events
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
        //server updates the position based on input
        if (IsServer)
        {
            Vector3 movement = new Vector3(currentInput.x, 0, currentInput.y) * moveSpeed * Time.deltaTime;
            transform.position += movement;
            

            networkPosition.Value = transform.position;
            
            //reset input after applying
            currentInput = Vector2.zero;
        }
        else
        {
            //clients interpolate to the networked position
            transform.position = Vector3.Lerp(transform.position, networkPosition.Value, Time.deltaTime * 10f);
        }

        //update view on all clients
        if (view != null)
        {
            view.UpdateVisuals(transform.position);
        }
    }

    public void SetMovementInput(Vector2 input)
    {
        //this should only be called on the server
        if (IsServer)
        {
            currentInput = input.normalized;
        }
    }
}
