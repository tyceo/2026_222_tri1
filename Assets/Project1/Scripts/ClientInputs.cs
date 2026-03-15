using UnityEngine;
using Unity.Netcode;

public class ClientInputs : NetworkBehaviour
{
    public NetworkVariable<int> score = new NetworkVariable<int>();
    
    private Model model;

    private void Start()
    {
        model = GetComponent<Model>();
    }

    public void Update()
    {
        //local only
        if (IsLocalPlayer)
        {
            //WASD input
            Vector2 moveInput = Vector2.zero;
            
            if (Input.GetKey(KeyCode.W)) moveInput.y += 1;
            if (Input.GetKey(KeyCode.S)) moveInput.y -= 1;
            if (Input.GetKey(KeyCode.A)) moveInput.x -= 1;
            if (Input.GetKey(KeyCode.D)) moveInput.x += 1;

            //send input to server
            if (moveInput != Vector2.zero)
            {
                SendMovementInput_ServerRpc(moveInput);
            }

            // New inputsystem is better
            //if (InputSystem.GetDevice<Keyboard>().spaceKey.wasPressedThisFrame)
            if(Input.GetKeyDown(KeyCode.Space))
            {
                GetBigOrDieTrying_RequestToServer_Rpc();
            }
        }
    }

    [Rpc(SendTo.Server, RequireOwnership = true)]
    private void SendMovementInput_ServerRpc(Vector2 input)
    {
        // Server receives input and updates the model
        if (model != null)
        {
            model.SetMovementInput(input);
        }
    }

    // Function that ONLY runs on the server. Typically for client controller code when they press buttons etc
    [Rpc(SendTo.Server, RequireOwnership = true)]
    private void GetBigOrDieTrying_RequestToServer_Rpc()
    {
        // This is running on the server. Check if it's legal/not cheating
        GetBigOrDieTrying_ServerToClients_Rpc();
    }


    // Function that runs from the Server TO ALL clients
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void GetBigOrDieTrying_ServerToClients_Rpc()
    {
        
        GetComponent<Renderer>().material.color = Color.red;
    }

}
