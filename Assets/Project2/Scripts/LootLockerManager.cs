using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using System;

public class LootLockerManager : MonoBehaviour
{
    public static LootLockerManager Instance { get; private set; }
    
    [Header("Leaderboard Settings")]
    [SerializeField] private string leaderboardKey = "my_leaderboard";
    
    private bool isSessionActive = false;
    public bool IsSessionActive => isSessionActive;
    
    public event Action OnSessionReady;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartLootLockerSession();
    }

    private void StartLootLockerSession()
    {
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (!response.success)
            {
                Debug.Log("error starting LootLocker session");
                isSessionActive = false;
                return;
            }

            Debug.Log("successfully started LootLocker session");
            isSessionActive = true;
            
            //set player name from saved data
            if (PlayerNameManager.Instance != null)
            {
                SetPlayerName(PlayerNameManager.Instance.PlayerName);
            }
            
            OnSessionReady?.Invoke();
        });
    }

    public void SetPlayerName(string name)
    {
        if (!isSessionActive)
        {
            Debug.LogWarning("LootLocker session not active, cannot set player name");
            return;
        }

        LootLockerSDKManager.SetPlayerName(name, (response) =>
        {
            if (!response.success)
            {
                Debug.Log("Could not set player name in LootLocker!");
                Debug.Log(response.errorData.ToString());
                return;
            }
            Debug.Log($"Successfully set player name in LootLocker: {name}");
        });
    }

    public void SubmitScore(int score)
    {
        if (!isSessionActive)
        {
            Debug.LogWarning("LootLocker session not active, cannot submit score");
            return;
        }

        string playerName = PlayerNameManager.Instance != null 
            ? PlayerNameManager.Instance.PlayerName 
            : "Player";

        LootLockerSDKManager.SubmitScore(playerName, score, leaderboardKey, (response) =>
        {
            if (!response.success)
            {
                Debug.Log("Could not submit score!");
                Debug.Log(response.errorData.ToString());
                return;
            }
            Debug.Log($"Successfully submitted score: {score} for {playerName}!");
        });
    }

    public void GetLeaderboard(int count, System.Action<LootLockerGetScoreListResponse> callback)
    {
        if (!isSessionActive)
        {
            Debug.LogWarning("LootLocker session not active, cannot get leaderboard");
            callback?.Invoke(null);
            return;
        }

        LootLockerSDKManager.GetScoreList(leaderboardKey, count, 0, (response) =>
        {
            if (!response.success)
            {
                Debug.Log("Could not get score list!");
                Debug.Log(response.errorData.ToString());
                callback?.Invoke(null);
                return;
            }
            Debug.Log("Successfully got score list!");
            callback?.Invoke(response);
        });
    }
}
