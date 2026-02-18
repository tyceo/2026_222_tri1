using UnityEngine;
using System;
using Unity.Netcode;
using Random = UnityEngine.Random;

public class FlickerLight : NetworkBehaviour
{
    public Light flickering;
    
        
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //only on server
        if (IsServer)
        {
            if (Random.value > 0.95f)
            {
               ToggleLight_Rpc(true);
               
            }
            else
            {
                ToggleLight_Rpc(false);
            }
            
        }
        
        
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void ToggleLight_Rpc(bool state)
    {
        flickering.enabled = state;
        
    }

    
}
