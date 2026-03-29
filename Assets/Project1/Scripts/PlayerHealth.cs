using UnityEngine;
using Unity.Netcode;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 3f;
    
    //syncs health across network
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(
        2f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private View view;

    private void Start()
    {
        view = GetComponent<View>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }
        
        //subscribe to health changes
        currentHealth.OnValueChanged += OnHealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        //unsubscribe from network variable callbacks
        currentHealth.OnValueChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(float previousValue, float newValue)
    {
        Debug.Log($"Health changed: {previousValue} -> {newValue}");
        
        UpdateHealthVisuals(newValue);
        
        if (newValue <= 0)
        {
            OnDeath();
        }
    }

    public void TakeDamage(float damage)
    {
        //only server processes damage
        if (!IsServer) return;
        
        currentHealth.Value = Mathf.Max(0, currentHealth.Value - damage);
        Debug.Log($"Player took {damage} damage. Health: {currentHealth.Value}/{maxHealth}");
    }

    public void Heal(float amount)
    {
        if (!IsServer) return;
        
        currentHealth.Value = Mathf.Min(maxHealth, currentHealth.Value + amount);
    }

    private void UpdateHealthVisuals(float health)
    {
        if (view != null)
        {
            float healthPercent = health / maxHealth;
            view.UpdateHealthColor(healthPercent);
        }
    }

    private void OnDeath()
    {
        Debug.Log($"Player {OwnerClientId} died!");
        
        if (IsServer)
        {
            //point to other player
            AwardPointToOtherPlayer();
            RespawnPlayer();
        }
    }

    private void AwardPointToOtherPlayer()
    {
        if (!IsServer) return;
        
        ClientInputs[] allPlayers = FindObjectsOfType<ClientInputs>();
        
        foreach (ClientInputs player in allPlayers)
        {
            //point to the player who is not this one
            if (player.OwnerClientId != OwnerClientId)
            {
                player.score.Value++;
            }
        }
    }

    private void RespawnPlayer()
    {
        if (!IsServer) return;
        
        currentHealth.Value = maxHealth;
        
        transform.position = new Vector3(0, 1, 0);
        
        Debug.Log("respawned");
    }

}
