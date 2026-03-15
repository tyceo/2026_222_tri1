using UnityEngine;
using Unity.Netcode;

public class Model : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    //networked position
    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
    
    //networked player name
    private NetworkVariable<NetworkString> playerName = new NetworkVariable<NetworkString>();
    
    private Vector2 currentInput;
    private View view;

    void Start()
    {
        view = GetComponent<View>();
    }

    void Update()
    {
        //server updates the position based on input
        if (IsServer)
        {
            Vector3 movement = new Vector3(currentInput.x, 0, currentInput.y) * moveSpeed * Time.deltaTime;
            transform.position += movement;
            

            networkPosition.Value = transform.position;
            
            //reset input after applying
            currentInput = Vector2.zero;
        }
        else
        {
            //clients interpolate to the networked position
            transform.position = Vector3.Lerp(transform.position, networkPosition.Value, Time.deltaTime * 10f);
        }

        //update view on all clients
        if (view != null)
        {
            view.UpdateVisuals(transform.position);
        }
    }

    public void SetMovementInput(Vector2 input)
    {
        //this should only be called on the server
        if (IsServer)
        {
            currentInput = input.normalized;
        }
    }
}

//struct for networking strings
public struct NetworkString : INetworkSerializable
{
    private string info;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref info);
    }

    public override string ToString()
    {
        return info;
    }

    public static implicit operator string(NetworkString s) => s.ToString();
    public static implicit operator NetworkString(string s) => new NetworkString() { info = s };
}
