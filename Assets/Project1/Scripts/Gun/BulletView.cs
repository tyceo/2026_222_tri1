using UnityEngine;

public class BulletView : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private GameObject bulletModel;
    
    void Start()
    {
        // Ensure visuals are active
        if (bulletModel != null)
        {
            bulletModel.SetActive(true);
        }
    }

    public void PlayHitEffect()
    {
        // Can add particle effects here
    }
}
