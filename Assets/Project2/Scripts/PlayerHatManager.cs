using UnityEngine;

public class PlayerHatManager : MonoBehaviour
{
    public static PlayerHatManager Instance { get; private set; }
    
    public int SelectedHatIndex { get; private set; } = 0;

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

    public void SetHat(int hatIndex)
    {
        SelectedHatIndex = hatIndex;
        Debug.Log($"Hat selected: {hatIndex}");
        
        //save for next time
        PlayerPrefs.SetInt("SelectedHat", hatIndex);
        PlayerPrefs.Save();
    }

    void Start()
    {
        //load saved hat if exists
        if (PlayerPrefs.HasKey("SelectedHat"))
        {
            SelectedHatIndex = PlayerPrefs.GetInt("SelectedHat");
        }
    }
}