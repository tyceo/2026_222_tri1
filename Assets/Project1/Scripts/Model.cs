using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class Model : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    [SerializeField] private bool useClientPrediction = true;
    [SerializeField] private float inputTimeout = 0.2f;
    
    [Header("Interpolation Settings")]
    [SerializeField] private bool useInterpolation = true;
    [SerializeField] private float interpolationTime = 0.03f;
    [SerializeField] private bool useExtrapolation = true;
    [SerializeField] private float extrapolationLimit = 1.5f;
    [SerializeField] private float snapThreshold = 30f;
    
    //network variables
    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    
    private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    
    private NetworkVariable<int> hatIndex = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    
    //input state
    private Vector2 currentInput;
    private float lastInputTime;
    
    //interpolation state
    private Vector3 positionVelocity;
    private Vector3 lastNetworkPosition;
    private Vector3 estimatedVelocity;
    
    //component references
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
        
        if (IsServer)
        {
            networkPosition.Value = transform.position;
        }
        
        lastNetworkPosition = networkPosition.Value;
        
        //client requests player data
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
        
        //server sets own player data
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
        }
        
        playerName.OnValueChanged += OnPlayerNameChanged;
        hatIndex.OnValueChanged += OnHatChanged;
        
        Invoke(nameof(InitializeNametag), 0.1f);
        Invoke(nameof(InitializeHat), 0.1f);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        playerName.OnValueChanged -= OnPlayerNameChanged;
        hatIndex.OnValueChanged -= OnHatChanged;
    }

    void Update()
    {
        //server authoritative movement
        if (IsServer)
        {
            if (Time.time - lastInputTime > inputTimeout)
            {
                currentInput = Vector2.zero;
            }
            
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            Vector3 movement = (right * currentInput.x + forward * currentInput.y) * moveSpeed * Time.deltaTime;
            
            transform.position += movement;
            networkPosition.Value = transform.position;
        }
        //local player client-side prediction
        else if (IsOwner && useClientPrediction)
        {
            float distance = Vector3.Distance(transform.position, networkPosition.Value);
            
            if (distance > snapThreshold)
            {
                transform.position = networkPosition.Value;
                positionVelocity = Vector3.zero;
                estimatedVelocity = Vector3.zero;
            }
            else if (distance > 0.1f)
            {
                transform.position = Vector3.Lerp(transform.position, networkPosition.Value, 0.1f);
            }
        }
        //remote players interpolation
        else if (!IsOwner)
        {
            float distance = Vector3.Distance(transform.position, networkPosition.Value);
            
            if (distance > snapThreshold)
            {
                transform.position = networkPosition.Value;
                positionVelocity = Vector3.zero;
                estimatedVelocity = Vector3.zero;
            }
            else if (useInterpolation)
            {
                if (networkPosition.Value != lastNetworkPosition)
                {
                    estimatedVelocity = (networkPosition.Value - lastNetworkPosition) / Time.deltaTime;
                    lastNetworkPosition = networkPosition.Value;
                }
                
                Vector3 targetPosition = networkPosition.Value;
                
                if (useExtrapolation)
                {
                    Vector3 extrapolation = estimatedVelocity * interpolationTime;
                    if (extrapolation.magnitude < extrapolationLimit)
                    {
                        targetPosition += extrapolation;
                    }
                }
                
                transform.position = Vector3.SmoothDamp(
                    transform.position, 
                    targetPosition, 
                    ref positionVelocity, 
                    interpolationTime
                );
            }
            else
            {
                transform.position = networkPosition.Value;
            }
        }

        if (view != null)
        {
            view.UpdateVisuals(transform.position);
        }
    }

    public void SetMovementInput(Vector2 input)
    {
        if (IsServer)
        {
            currentInput = input.normalized;
            lastInputTime = Time.time;
        }
        
        if (!IsServer && IsOwner && useClientPrediction)
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            Vector3 movement = (right * input.x + forward * input.y) * moveSpeed * Time.deltaTime;
            transform.position += movement;
        }
    }

    public void ForceSetPosition(Vector3 position)
    {
        if (IsServer)
        {
            transform.position = position;
            networkPosition.Value = position;
        }
    }

    public string GetPlayerName()
    {
        return playerName.Value.ToString();
    }

    //server rpc for setting player data
    [ServerRpc]
    private void RequestSetPlayerDataServerRpc(string name, int hat, ServerRpcParams rpcParams = default)
    {
        if (name.Length > 12)
        {
            name = name.Substring(0, 12);
        }
        
        if (string.IsNullOrEmpty(name))
        {
            name = $"Player{OwnerClientId}";
        }
        
        if (hat < 0 || hat > 1)
        {
            hat = 0;
        }
        
        playerName.Value = name;
        hatIndex.Value = hat;
    }

    //network variable callbacks
    private void OnPlayerNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        if (view != null)
        {
            view.SetPlayerName(newValue.ToString());
        }
    }

    private void OnHatChanged(int previousValue, int newValue)
    {
        if (view != null)
        {
            view.SetHat(newValue);
        }
    }

    //initialization helpers
    private void InitializeNametag()
    {
        if (view != null)
        {
            view.SetPlayerName(playerName.Value.ToString());
        }
    }

    private void InitializeHat()
    {
        if (view != null)
        {
            view.SetHat(hatIndex.Value);
        }
    }
}