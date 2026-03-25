using UnityEngine;
using Unity.Netcode;

public class GunModel : NetworkBehaviour
{
    [Header("Gun Settings")]
    public int maxAmmo = 3;
    public int currentAmmo = 3;
    public float damage = 1f;
    public float fireRate = 0.5f; // 0.5 second cooldown
    public float range = 100f;
    
    [Header("Bullet Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform muzzlePoint;

    //syncs equipped state across network
    private NetworkVariable<bool> isEquipped = new NetworkVariable<bool>(false);
    //syncs ammo count across network
    private NetworkVariable<int> ammoCount = new NetworkVariable<int>(3);
    
    private float lastFireTime;
    private GunView gunView;
    private PlayerInventory ownerInventory;

    void Awake()
    {
        gunView = GetComponent<GunView>();
        ammoCount.Value = currentAmmo;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        //subscribe to network variable changes
        isEquipped.OnValueChanged += OnEquippedStateChanged;
        ammoCount.OnValueChanged += OnAmmoChanged;
        
        UpdateVisuals();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        //unsubscribe from network variable callbacks
        isEquipped.OnValueChanged -= OnEquippedStateChanged;
        ammoCount.OnValueChanged -= OnAmmoChanged;
    }

    private void OnEquippedStateChanged(bool previousValue, bool newValue)
    {
        UpdateVisuals();
    }

    private void OnAmmoChanged(int previousValue, int newValue)
    {
        currentAmmo = newValue;
        if (gunView != null)
        {
            gunView.UpdateAmmoDisplay(newValue, maxAmmo);
        }

        //server destroys gun when ammo reaches 0
        if (newValue <= 0 && IsServer)
        {
            DestroyGun();
        }
    }

    private void UpdateVisuals()
    {
        if (gunView != null)
        {
            gunView.SetEquippedState(isEquipped.Value);
        }
    }

    public bool TryShoot(out Vector3 shootPosition, out Vector3 shootDirection)
    {
        shootPosition = Vector3.zero;
        shootDirection = Vector3.forward;
        
        //only server validates shooting
        if (!IsServer) return false;
        
        if (Time.time - lastFireTime < fireRate)
            return false;
        
        if (ammoCount.Value <= 0)
            return false;
        
        lastFireTime = Time.time;
        ammoCount.Value--;
        
        if (muzzlePoint != null)
        {
            shootPosition = muzzlePoint.position;
            shootDirection = muzzlePoint.forward;
        }
        
        return true;
    }

    public void SpawnBullet(Vector3 position, Vector3 direction, ulong shooterId)
    {
        //only server spawns bullets
        if (!IsServer) return;
        if (bulletPrefab == null) return;

        GameObject bulletObj = Instantiate(bulletPrefab, position, Quaternion.LookRotation(direction));
        NetworkObject bulletNetObj = bulletObj.GetComponent<NetworkObject>();
        
        if (bulletNetObj != null)
        {
            //spawn bullet on network for all clients to see
            bulletNetObj.Spawn();
            
            BulletModel bullet = bulletObj.GetComponent<BulletModel>();
            if (bullet != null)
            {
                bullet.Initialize(direction, shooterId);
            }
        }
    }

    public void SetEquipped(bool equipped, PlayerInventory inventory = null)
    {
        if (IsServer)
        {
            isEquipped.Value = equipped;
            ownerInventory = inventory;
        }
    }

    public bool IsEquipped()
    {
        return isEquipped.Value;
    }

    public void Reload()
    {
        if (IsServer)
        {
            ammoCount.Value = maxAmmo;
            currentAmmo = maxAmmo;
        }
    }

    private void DestroyGun()
    {
        if (!IsServer) return;

        Debug.Log("Gun out of ammo - destroying gun");

        if (isEquipped.Value && ownerInventory != null)
        {
            ownerInventory.ClearGunReference();
        }

        //despawn gun from network, destroy on all clients
        NetworkObject netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
        {
            netObj.Despawn(true);
        }
    }
}
