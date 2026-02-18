using UnityEngine;
using Unity.Netcode;

public class ClientInputs : NetworkBehaviour
{
    public NetworkVariable<int> score = new NetworkVariable<int>();
    public void Update()
    {
        // Local only. Not networked
        if (IsLocalPlayer)
        {
        // New inputsystem is better
        //if (InputSystem.GetDevice<Keyboard>().spaceKey.wasPressedThisFrame)
            if(Input.GetKeyDown(KeyCode.Space))
            {
                GetBigOrDieTrying_RequestToServer_Rpc();
            }
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
