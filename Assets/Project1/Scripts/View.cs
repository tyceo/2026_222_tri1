using UnityEngine;
using TMPro;

public class View : MonoBehaviour
{
    [Header("Nametag Settings")]
    [SerializeField] private float nametagHeight = 1.5f;
    [SerializeField] private float nametagScale = 0.01f;
    
    
    private GameObject nametagObject;
    private TextMeshPro nametagText;
    private Camera mainCamera;
    private Renderer playerRenderer;
    private TextMeshProUGUI hostScoreText;
    private TextMeshProUGUI clientScoreText;

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
        //find all players and get their scores
        ClientInputs[] players = FindObjectsOfType<ClientInputs>();
        
        int hostScore = 0;
        int clientScore = 0;
        
        foreach (ClientInputs player in players)
        {
            if (player.OwnerClientId == 0)
            {
                hostScore = player.score.Value;
            }
            else
            {
                clientScore = player.score.Value;
            }
        }
        
        UpdateScores(hostScore, clientScore);
    }

    public void UpdateScores(int hostScore, int clientScore)
    {
        if (hostScoreText != null)
        {
            hostScoreText.text = $"Host: {hostScore}";
        }
        
        if (clientScoreText != null)
        {
            clientScoreText.text = $"Client: {clientScore}";
        }
    }
}
