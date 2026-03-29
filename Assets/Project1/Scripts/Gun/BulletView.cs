using UnityEngine;

public class BulletView : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private GameObject bulletModel;
    
    void Start()
    {
        //ensure visuals are active
        if (bulletModel != null)
        {
            bulletModel.SetActive(true);
        }
    }

    public void PlayHitEffect()
    {
        //particle effect
    }
}
