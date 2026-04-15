using UnityEngine;
using TMPro;

public class View : MonoBehaviour
{
    [Header("Nametag Settings")]
    [SerializeField] private float nametagHeight = 1.5f;
    [SerializeField] private float nametagScale = 0.01f;
    
    [Header("Hat Settings")]
    [SerializeField] private GameObject[] hatPrefabs; //inspector
    [SerializeField] private Transform hatSocket; //where the hat sits on the player
    
    private GameObject nametagObject;
    private TextMeshPro nametagText;
    private Camera mainCamera;
    private Renderer playerRenderer;
    private TextMeshProUGUI hostScoreText;
    private TextMeshProUGUI clientScoreText;
    private GameObject currentHat;

    void Start()
    {
        mainCamera = Camera.main;
        playerRenderer = GetComponent<Renderer>();
        
        GameObject hostScoreObj = GameObject.Find("HostScore");
        if (hostScoreObj != null)
        {
            hostScoreText = hostScoreObj.GetComponent<TextMeshProUGUI>();
        }
        
        GameObject clientScoreObj = GameObject.Find("ClientScore");
        if (clientScoreObj != null)
        {
            clientScoreText = clientScoreObj.GetComponent<TextMeshProUGUI>();
        }
    }

    void Update()
    {
        //nametag face the camera
        if (nametagObject != null && mainCamera != null)
        {
            nametagObject.transform.LookAt(nametagObject.transform.position + mainCamera.transform.rotation * Vector3.forward,
                                          mainCamera.transform.rotation * Vector3.up);
        }
        UpdateScoreDisplay();
    }

    private void CreateNametag()
    {
        //create nametag GameObject
        nametagObject = new GameObject("Nametag");
        nametagObject.tag = "NameTag"; //set NameTag tag
        nametagObject.transform.SetParent(transform);
        nametagObject.transform.localPosition = new Vector3(0, nametagHeight, 0);
        nametagObject.transform.localScale = Vector3.one * nametagScale;
        
        nametagText = nametagObject.AddComponent<TextMeshPro>();
        nametagText.text = "Player";
        nametagText.fontSize = 36;
        nametagText.alignment = TextAlignmentOptions.Center;
        nametagText.color = Color.white;
        
        //can't see it, trying outline (was just too small but this looks good)
        nametagText.outlineWidth = 0.2f;
        nametagText.outlineColor = Color.black;
    }

    public void SetPlayerName(string playerName)
    {
        if (nametagObject == null)
        {
            CreateNametag();
        }
        
        if (nametagText != null)
        {
            nametagText.text = playerName;
        }
    }

    public void SetHat(int hatIndex)
    {
        //remove current hat if exists
        if (currentHat != null)
        {
            Destroy(currentHat);
            currentHat = null;
        }
        
        //validate hat index
        if (hatPrefabs == null || hatIndex < 0 || hatIndex >= hatPrefabs.Length)
        {
            Debug.LogWarning($"Invalid hat index: {hatIndex}");
            return;
        }
        
        if (hatPrefabs[hatIndex] == null)
        {
            Debug.LogWarning($"Hat prefab at index {hatIndex} is null");
            return;
        }
        
        //create new hat
        Transform parent = hatSocket != null ? hatSocket : transform;
        currentHat = Instantiate(hatPrefabs[hatIndex], parent);
        currentHat.transform.localPosition = Vector3.zero;
        currentHat.transform.localRotation = Quaternion.identity;
        
        Debug.Log($"Hat {hatIndex} equipped");
    }

    public void UpdateVisuals(Vector3 position)
    {

    }

    public void UpdateHealthColor(float healthPercent)
    {
        if (playerRenderer != null)
        {
            playerRenderer.material.color = Color.Lerp(Color.red, Color.green, healthPercent);
        }
    }

    private void UpdateScoreDisplay()
    {
        //find all players and get their scores and names
        Model[] players = FindObjectsOfType<Model>();
        
        int hostScore = 0;
        int clientScore = 0;
        string hostName = "Host";
        string clientName = "Client";
        
        foreach (Model player in players)
        {
            ClientInputs clientInputs = player.GetComponent<ClientInputs>();
            if (clientInputs != null)
            {
                if (player.OwnerClientId == 0)
                {
                    hostScore = clientInputs.score.Value;
                    hostName = player.GetPlayerName();
                }
                else
                {
                    clientScore = clientInputs.score.Value;
                    clientName = player.GetPlayerName();
                }
            }
        }
        
        UpdateScores(hostScore, clientScore, hostName, clientName);
    }

    public void UpdateScores(int hostScore, int clientScore, string hostName, string clientName)
    {
        if (hostScoreText != null)
        {
            hostScoreText.text = $"{hostName}: {hostScore}";
        }
        
        if (clientScoreText != null)
        {
            clientScoreText.text = $"{clientName}: {clientScore}";
        }
    }
}