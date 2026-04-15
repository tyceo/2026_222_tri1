using UnityEngine;

public class PlayerNameManager : MonoBehaviour
{
    public static PlayerNameManager Instance { get; private set; }
    
    public string PlayerName { get; private set; } = "Player";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetPlayerName(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            PlayerName = name;
            Debug.Log($"Player name set to: {PlayerName}");
        }
    }
}