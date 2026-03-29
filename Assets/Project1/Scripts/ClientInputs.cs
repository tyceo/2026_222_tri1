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
            }
        }
    }
}
