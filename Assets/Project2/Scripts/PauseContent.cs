using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Lobbies;
using Unity.Services.Authentication;
using Unity.Netcode;
using System.Threading.Tasks;

public class PauseContent : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuGameObject;
    [SerializeField] private GameObject blockPauseIfActive;

    void Start()
    {

        if (pauseMenuGameObject != null)
        {
            pauseMenuGameObject.SetActive(false);
        }
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OpenPauseMenu();
        }
    }

    private void OpenPauseMenu()
    {

        if (blockPauseIfActive != null && blockPauseIfActive.activeSelf)
        {
            return;
        }
        
        if (pauseMenuGameObject != null && !pauseMenuGameObject.activeSelf)
        {
            pauseMenuGameObject.SetActive(true);
            

            Time.timeScale = 0f;
            

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }


    public async void ReturnToMenu()
    {
        Time.timeScale = 1f;
        

        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        //cleanup lobby and netcode before reloading scene
        await CleanupLobbyAndNetcode();
        
        //use LoadSceneMode.Single to ensure clean reload
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }


    public void ClosePauseMenu()
    {
        if (pauseMenuGameObject != null)
        {
            pauseMenuGameObject.SetActive(false);
            

            Time.timeScale = 1f;
            

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }


    public async void QuitGame()
    {
        Time.timeScale = 1f;

        //cleanup lobby and netcode before quitting
        await CleanupLobbyAndNetcode();
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private async Task CleanupLobbyAndNetcode()
    {
        bool isHost = NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
        
        //get lobby info before shutting down
        var createLobby = FindObjectOfType<CreateLobby>();
        var joinLobby = FindObjectOfType<JoinLobby>();
        string lobbyId = null;
        
 
        if (createLobby != null)
        {
            lobbyId = GetLobbyId(createLobby);
        }
        
     
        if (string.IsNullOrEmpty(lobbyId) && joinLobby != null)
        {
            lobbyId = GetJoinedLobbyId(joinLobby);
        }


        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        //handle lobby cleanup based on role
        if (!string.IsNullOrEmpty(lobbyId))
        {
            try
            {
                if (isHost)
                {
                    //host deletes the entire lobby
                    await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
                    Debug.Log("Lobby deleted successfully (Host)");
                }
                else
                {
                    //client removes themselves from the lobby
                    string playerId = AuthenticationService.Instance.PlayerId;
                    await LobbyService.Instance.RemovePlayerAsync(lobbyId, playerId);
                    Debug.Log("Left lobby successfully (Client)");
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning($"Failed to cleanup lobby: {e}");
            }
        }
    }

    private string GetLobbyId(CreateLobby createLobby)
    {
        
        var field = typeof(CreateLobby).GetField("currentLobby", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            var lobby = field.GetValue(createLobby) as Unity.Services.Lobbies.Models.Lobby;
            return lobby?.Id;
        }
        
        return null;
    }

    private string GetJoinedLobbyId(JoinLobby joinLobby)
    {

        var field = typeof(JoinLobby).GetField("joinedLobby", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            var lobby = field.GetValue(joinLobby) as Unity.Services.Lobbies.Models.Lobby;
            return lobby?.Id;
        }
        
        return null;
    }
}