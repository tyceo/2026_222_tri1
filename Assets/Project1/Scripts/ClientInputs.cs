using UnityEngine;
using Unity.Netcode;

public class ClientInputs : NetworkBehaviour
{
    public NetworkVariable<int> score = new NetworkVariable<int>();
    
    private Model model;
    private PlayerInventory inventory;
    private FirstPersonCamera fpsCamera;

    private void Start()
    {
        model = GetComponent<Model>();
        inventory = GetComponent<PlayerInventory>();
        fpsCamera = GetComponent<FirstPersonCamera>();
    }

    public void Update()
    {
        //only local player sends input
        if (IsLocalPlayer)
        {
            Vector2 moveInput = Vector2.zero;
            
            if (Input.GetKey(KeyCode.W)) moveInput.y += 1;
            if (Input.GetKey(KeyCode.S)) moveInput.y -= 1;
            if (Input.GetKey(KeyCode.A)) moveInput.x -= 1;
            if (Input.GetKey(KeyCode.D)) moveInput.x += 1;

            //send movement input to server via rpc
            if (moveInput != Vector2.zero)
            {
                SendMovementInput_ServerRpc(moveInput);
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (fpsCamera != null)
                {
                    Camera cam = fpsCamera.GetCamera();
                    if (cam != null)
                    {
                        RequestShoot_ServerRpc(cam.transform.position, cam.transform.forward);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                RequestReload_ServerRpc();
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                RequestDropGun_ServerRpc();
            }

            if(Input.GetKeyDown(KeyCode.Space))
            {
                GetBigOrDieTrying_RequestToServer_Rpc();
            }
        }
    }

    //rpc: client sends input to server
    [Rpc(SendTo.Server)]
    private void SendMovementInput_ServerRpc(Vector2 input)
    {
        if (model != null)
        {
            model.SetMovementInput(input);
        }
    }

    //rpc: client requests to shoot, server validates and spawns bullet
    [Rpc(SendTo.Server)]
    private void RequestShoot_ServerRpc(Vector3 cameraPosition, Vector3 cameraDirection)
    {
        if (inventory == null) return;
        
        GunModel gun = inventory.GetEquippedGun();
        if (gun != null)
        {
            Vector3 shootPosition, shootDirection;
            
            if (gun.TryShoot(out shootPosition, out shootDirection))
            {
                gun.SpawnBullet(shootPosition, cameraDirection, OwnerClientId);
                
                //rpc to all clients to show visual effect
                ShowShootEffect_ClientRpc(shootPosition, cameraDirection);
            }
        }
    }

    //rpc: server tells all clients to play shoot effect
    [Rpc(SendTo.Everyone)]
    private void ShowShootEffect_ClientRpc(Vector3 position, Vector3 direction)
    {
        if (inventory == null) return;
        
        GunModel gun = inventory.GetEquippedGun();
        if (gun != null)
        {
            GunView gunView = gun.GetComponent<GunView>();
            if (gunView != null)
            {
                Vector3 hitPoint = position + direction * 100f;
                gunView.PlayShootEffect(hitPoint);
            }
        }
    }

    //rpc: client requests reload, server validates
    [Rpc(SendTo.Server)]
    private void RequestReload_ServerRpc()
    {
        if (inventory == null) return;
        
        GunModel gun = inventory.GetEquippedGun();
        if (gun != null)
        {
            gun.Reload();
            ShowReloadEffect_ClientRpc();
        }
    }

    //rpc: server tells all clients to play reload effect
    [Rpc(SendTo.Everyone)]
    private void ShowReloadEffect_ClientRpc()
    {
        if (inventory == null) return;
        
        GunModel gun = inventory.GetEquippedGun();
        if (gun != null)
        {
            GunView gunView = gun.GetComponent<GunView>();
            if (gunView != null)
            {
                gunView.PlayReloadEffect();
            }
        }
    }

    //rpc: client requests to drop gun
    [Rpc(SendTo.Server)]
    private void RequestDropGun_ServerRpc()
    {
        if (inventory != null)
        {
            inventory.DropGun();
        }
    }

    [Rpc(SendTo.Server)]
    private void GetBigOrDieTrying_RequestToServer_Rpc()
    {
        GetBigOrDieTrying_ServerToClients_Rpc();
    }

    //rpc: server sends to all clients
    [Rpc(SendTo.ClientsAndHost)]
    private void GetBigOrDieTrying_ServerToClients_Rpc()
    {
        GetComponent<Renderer>().material.color = Color.red;
    }
}
