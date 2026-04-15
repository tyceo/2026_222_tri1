using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LobbyListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private Button joinButton;
    
    private string lobbyId;
    private LobbyBrowser lobbyBrowser;

    public void Setup(string id, string lobbyName, int currentPlayers, int maxPlayers, LobbyBrowser browser)
    {
        lobbyId = id;
        lobbyBrowser = browser;
        
        lobbyNameText.text = lobbyName;
        playerCountText.text = $"{currentPlayers}/{maxPlayers}";
        
        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(OnJoinClicked);
    }

    private void OnJoinClicked()
    {
        lobbyBrowser.JoinLobbyById(lobbyId);
    }
}