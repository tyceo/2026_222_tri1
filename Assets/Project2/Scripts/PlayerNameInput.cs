using UnityEngine;
using TMPro;

public class PlayerNameInput : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputField;
    
    void Start()
    {
        //load saved name if exists
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            string savedName = PlayerPrefs.GetString("PlayerName");
            nameInputField.text = savedName;
            PlayerNameManager.Instance.SetPlayerName(savedName);
        }
    }

    public void OnNameChanged()
    {
        string newName = nameInputField.text;
        
        //limit name length
        if (newName.Length > 12)
        {
            newName = newName.Substring(0, 12);
            nameInputField.text = newName;
        }
        
        PlayerNameManager.Instance.SetPlayerName(newName);
        
        //save for next time
        PlayerPrefs.SetString("PlayerName", newName);
        PlayerPrefs.Save();
    }
}