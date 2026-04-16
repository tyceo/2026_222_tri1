using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseContent : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuGameObject;
    [SerializeField] private GameObject blockPauseIfActive;

    void Start()
    {
        //pause menu is hidden at start
        if (pauseMenuGameObject != null)
        {
            pauseMenuGameObject.SetActive(false);
        }
    }

    void Update()
    {
        //Open pause menu with ESC key (only opens, doesn't close)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OpenPauseMenu();
        }
    }

    private void OpenPauseMenu()
    {
        //Check if pause should be blocked
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


    public void ReturnToMenu()
    {

        Time.timeScale = 1f;
        

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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


    public void QuitGame()
    {

        Time.timeScale = 1f;
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}