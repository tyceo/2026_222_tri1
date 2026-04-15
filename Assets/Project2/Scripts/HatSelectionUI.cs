using UnityEngine;
using UnityEngine.UI;

public class HatSelectionUI : MonoBehaviour
{
    [SerializeField] private Button hat1Button;
    [SerializeField] private Button hat2Button;
    [SerializeField] private Button hat3Button;
    [SerializeField] private Button hat4Button;
    [SerializeField] private Button hat5Button;
    [SerializeField] private Button hat6Button;

    void Start()
    {
        hat1Button.onClick.AddListener(() => PlayerHatManager.Instance.SetHat(0));
        hat2Button.onClick.AddListener(() => PlayerHatManager.Instance.SetHat(1));
        hat3Button.onClick.AddListener(() => PlayerHatManager.Instance.SetHat(2));
        hat4Button.onClick.AddListener(() => PlayerHatManager.Instance.SetHat(3));
        hat5Button.onClick.AddListener(() => PlayerHatManager.Instance.SetHat(4));
        hat6Button.onClick.AddListener(() => PlayerHatManager.Instance.SetHat(5));
    }
}