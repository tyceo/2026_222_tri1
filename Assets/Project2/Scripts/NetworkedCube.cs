using UnityEngine;
using Unity.Netcode;

public class NetworkedCube : NetworkBehaviour
{
    public NetworkVariable<int> health = new NetworkVariable<int>(100);

    void Update()
    {
        //client sends request every frame
        if (!IsServer)
        {
            ChangeHealthServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ChangeHealthServerRpc()
    {
        health.Value = Random.Range(0, 100);
    }
}