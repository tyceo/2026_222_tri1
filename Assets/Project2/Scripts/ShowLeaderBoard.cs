using UnityEngine;
using LootLocker.Requests;
using TMPro;
using System.Collections.Generic;

public class ShowLeaderBoard : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject leaderboardEntryPrefab;
    [SerializeField] private Transform leaderboardContainer;
    [SerializeField] private TextMeshProUGUI loadingText;
    
    [Header("Leaderboard Settings")]
    [SerializeField] private int maxEntries = 10;
    
    private List<GameObject> spawnedEntries = new List<GameObject>();

    void Start()
    {
        if (LootLockerManager.Instance != null)
        {
            if (LootLockerManager.Instance.IsSessionActive)
            {
                LoadLeaderboard();
            }
            else
            {
                // Wait for session to be ready
                LootLockerManager.Instance.OnSessionReady += LoadLeaderboard;
                
                if (loadingText != null)
                {
                    loadingText.text = "Connecting...";
                    loadingText.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            Debug.LogError("LootLockerManager instance not found!");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from event to prevent memory leaks
        if (LootLockerManager.Instance != null)
        {
            LootLockerManager.Instance.OnSessionReady -= LoadLeaderboard;
        }
    }

    public void LoadLeaderboard()
    {
        if (loadingText != null)
        {
            loadingText.text = "Loading...";
            loadingText.gameObject.SetActive(true);
        }
        
        ClearLeaderboard();
        
        LootLockerManager.Instance.GetLeaderboard(maxEntries, (response) =>
        {
            if (loadingText != null)
            {
                loadingText.gameObject.SetActive(false);
            }
            
            if (response != null && response.items != null)
            {
                DisplayLeaderboard(response);
            }
            else
            {
                Debug.LogWarning("Failed to load leaderboard");
            }
        });
    }

    private void DisplayLeaderboard(LootLockerGetScoreListResponse response)
    {
        foreach (LootLockerLeaderboardMember entry in response.items)
        {
            GameObject entryObject;
            
            if (leaderboardEntryPrefab != null && leaderboardContainer != null)
            {
                entryObject = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
            }
            else
            {
                entryObject = new GameObject($"Entry_{entry.rank}");
                entryObject.transform.SetParent(transform);
            }
            
            TextMeshProUGUI textComponent = entryObject.GetComponent<TextMeshProUGUI>();
            if (textComponent == null)
            {
                textComponent = entryObject.AddComponent<TextMeshProUGUI>();
            }
            
            // Use player name if available, otherwise fallback to member_id
            string displayName = !string.IsNullOrEmpty(entry.player.name) ? entry.player.name : entry.member_id;
            textComponent.text = $"{entry.rank}. {displayName} - {entry.score}";
            textComponent.fontSize = 15;
            textComponent.color = Color.white;
            
            spawnedEntries.Add(entryObject);
            
            Debug.Log($"{entry.rank}. {displayName}: {entry.score}");
        }
    }

    private void ClearLeaderboard()
    {
        foreach (GameObject entry in spawnedEntries)
        {
            Destroy(entry);
        }
        spawnedEntries.Clear();
    }

    public void RefreshLeaderboard()
    {
        LoadLeaderboard();
    }
}