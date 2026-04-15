using UnityEngine;

public class PlayerNameManager : MonoBehaviour
{
    public static PlayerNameManager Instance { get; private set; }
    
    private const string PLAYER_NAME_KEY = "SavedPlayerName";
    
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
        
        LoadPlayerName();
    }

    private void LoadPlayerName()
    {
        if (PlayerPrefs.HasKey(PLAYER_NAME_KEY))
        {
            PlayerName = PlayerPrefs.GetString(PLAYER_NAME_KEY);
            Debug.Log($"Loaded saved player name: {PlayerName}");
        }
        else
        {
            PlayerName = "Player";
            Debug.Log("No saved name found, using default");
        }
    }

    public void SetPlayerName(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            PlayerName = name;
            PlayerPrefs.SetString(PLAYER_NAME_KEY, name);
            PlayerPrefs.Save();
            Debug.Log($"Player name set and saved: {PlayerName}");
            
            // Update LootLocker player name if session is active
            if (LootLockerManager.Instance != null && LootLockerManager.Instance.IsSessionActive)
            {
                LootLockerManager.Instance.SetPlayerName(name);
            }
        }
    }
}