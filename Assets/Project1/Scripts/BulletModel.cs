using UnityEngine;
using Unity.Netcode;

public class BulletModel : NetworkBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 20f;
    public float damage = 1f;
    public float lifetime = 5f;
    
    //syncs bullet position across network
    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    
    private ulong shooterClientId;
    private Vector3 direction;
    private float spawnTime;
    private bool isInitialized = false;

    public void Initialize(Vector3 shootDirection, ulong shooterId)
    {
        direction = shootDirection.normalized;
        shooterClientId = shooterId;
        spawnTime = Time.time;
        isInitialized = true;
        
        if (IsServer)
        {
            networkPosition.Value = transform.position;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        //clients subscribe to position changes
        if (!IsServer)
        {
            networkPosition.OnValueChanged += OnPositionChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        if (!IsServer)
        {
            networkPosition.OnValueChanged -= OnPositionChanged;
        }
    }

    private void OnPositionChanged(Vector3 previousValue, Vector3 newValue)
    {
        //clients update position from network
        transform.position = newValue;
    }

    void Update()
    {
        if (!isInitialized) return;

        if (IsServer)
        {
            //server moves bullet and syncs position
            transform.position += direction * speed * Time.deltaTime;
            networkPosition.Value = transform.position;

            if (Time.time - spawnTime > lifetime)
            {
                DestroyBullet();
            }
        }
        else
        {
            //clients interpolate to networked position
            transform.position = Vector3.Lerp(transform.position, networkPosition.Value, Time.deltaTime * 20f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //only server processes collisions
        if (!IsServer) return;

        NetworkObject hitNetObj = other.GetComponent<NetworkObject>();
        if (hitNetObj != null && hitNetObj.OwnerClientId == shooterClientId)
            return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log($"Bullet hit player for {damage} damage!");
        }

        DestroyBullet();
    }

    private void DestroyBullet()
    {
        if (!IsServer) return;

        //despawn bullet from network
        NetworkObject netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
        {
            netObj.Despawn(true);
        }
    }
}
