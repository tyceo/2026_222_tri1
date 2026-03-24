using UnityEngine;
using Unity.Netcode;

public class PlayerInventory : NetworkBehaviour
{
    [Header("Gun Settings")]
    [SerializeField] private Transform gunHolder;
    
    //syncs equipped gun id across network
    private NetworkVariable<ulong> equippedGunNetworkId = new NetworkVariable<ulong>(
        ulong.MaxValue,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    
    private GunPickup currentGun;
    private NetworkObject playerNetworkObject;

    void Awake()
    {
        playerNetworkObject = GetComponent<NetworkObject>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        //subscribe to equipped gun changes
        equippedGunNetworkId.OnValueChanged += OnEquippedGunChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        //unsubscribe from network variable callbacks
        equippedGunNetworkId.OnValueChanged -= OnEquippedGunChanged;
    }

    void Update()
    {
        if (currentGun != null && gunHolder != null)
        {
            currentGun.transform.position = gunHolder.position;
            currentGun.transform.rotation = gunHolder.rotation;
        }
    }

    private void OnEquippedGunChanged(ulong previousValue, ulong newValue)
    {
        //update local gun reference when network variable changes
        if (newValue != ulong.MaxValue)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(newValue, out NetworkObject gunNetObj))
            {
                currentGun = gunNetObj.GetComponent<GunPickup>();
            }
        }
        else
        {
            currentGun = null;
        }
    }

    public bool TryEquipGun(GunPickup gun)
    {
        //only server equips guns
        if (!IsServer) return false;
        
        if (currentGun != null)
        {
            DropGun();
        }

        currentGun = gun;
        
        gun.OnPickedUp(gunHolder, playerNetworkObject);
        
        //sync gun ownership across network
        NetworkObject gunNetObj = gun.GetComponent<NetworkObject>();
        if (gunNetObj != null)
        {
            equippedGunNetworkId.Value = gunNetObj.NetworkObjectId;
            
            GunModel gunModel = gun.GetComponent<GunModel>();
            if (gunModel != null)
            {
                gunModel.SetEquipped(true, this);
            }
        }

        return true;
    }

    public void DropGun()
    {
        //only server drops guns
        if (!IsServer) return;
        if (currentGun == null) return;

        Vector3 dropPosition = transform.position + transform.forward * 2f;
        currentGun.OnDropped(dropPosition);
        
        currentGun = null;
        equippedGunNetworkId.Value = ulong.MaxValue;
    }

    public void ClearGunReference()
    {
        if (!IsServer) return;
        
        currentGun = null;
        equippedGunNetworkId.Value = ulong.MaxValue;
    }

    public GunModel GetEquippedGun()
    {
        if (currentGun != null)
        {
            return currentGun.GetComponent<GunModel>();
        }
        return null;
    }

    public bool HasGun()
    {
        return currentGun != null;
    }
}
