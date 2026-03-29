using UnityEngine;
using Unity.Netcode;

public class GunPickup : NetworkBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private float pickupRadius = 2f;
    [SerializeField] private LayerMask playerLayer;
    
    private GunModel gunModel;
    private bool isPickedUp = false;

    void Awake()
    {
        gunModel = GetComponent<GunModel>();
    }

    void OnTriggerEnter(Collider other)
    {
        //nnly process on server
        if (!IsServer) return;
        if (isPickedUp) return;

        //check if player
        PlayerInventory playerInventory = other.GetComponent<PlayerInventory>();
        if (playerInventory != null)
        {

            if (playerInventory.TryEquipGun(this))
            {
                isPickedUp = true;
            }
        }
    }

    public void OnPickedUp(Transform parent, NetworkObject playerNetworkObject)
    {
        if (!IsServer) return;

        NetworkObject gunNetObj = GetComponent<NetworkObject>();
        if (gunNetObj != null && playerNetworkObject != null)
        {
            //parent gun to player networkobject
            gunNetObj.TrySetParent(playerNetworkObject, false);
            
            transform.position = parent.position;
            transform.rotation = parent.rotation;
            
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }
        }
    }

    public void OnDropped(Vector3 dropPosition)
    {
        if (!IsServer) return;

        isPickedUp = false;
        
        NetworkObject gunNetObj = GetComponent<NetworkObject>();
        if (gunNetObj != null)
        {
            //unparent gun from player networkobject
            gunNetObj.TrySetParent((NetworkObject)null, false);
            
            transform.position = dropPosition;
        }
        
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }

        //set gun as not equipped
        if (gunModel != null)
        {
            gunModel.SetEquipped(false);
        }
    }
}
