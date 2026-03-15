using Unity.Netcode;  // Needed for Netcode for GameObjects
using UnityEngine;

public class NetworkMenu : MonoBehaviour
{
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log("Started as Host");
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("Started as Client");
    }
}
