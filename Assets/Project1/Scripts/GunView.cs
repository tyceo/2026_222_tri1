using UnityEngine;

public class GunView : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private GameObject gunModel;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private LineRenderer bulletTrail;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip reloadSound;

    void Awake()
    {
        if (bulletTrail != null)
        {
            bulletTrail.enabled = false;
        }
        
        // Always show gun model
        if (gunModel != null)
        {
            gunModel.SetActive(true);
        }
    }

    public void SetEquippedState(bool equipped)
    {
        // Gun visuals are always visible - do nothing
        // You can add other visual changes here if needed (e.g., highlighting)
    }

    public void PlayShootEffect(Vector3 hitPoint)
    {
        // Muzzle flash
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // Sound
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        // Bullet trail
        if (bulletTrail != null && muzzlePoint != null)
        {
            StartCoroutine(ShowBulletTrail(muzzlePoint.position, hitPoint));
        }
    }

    private System.Collections.IEnumerator ShowBulletTrail(Vector3 start, Vector3 end)
    {
        bulletTrail.enabled = true;
        bulletTrail.SetPosition(0, start);
        bulletTrail.SetPosition(1, end);
        
        yield return new WaitForSeconds(0.05f);
        
        bulletTrail.enabled = false;
    }

    public void PlayReloadEffect()
    {
        if (audioSource != null && reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }
    }

    public void UpdateAmmoDisplay(int current, int max)
    {
        // This can update UI or other visual feedback
        // For now just debug
        Debug.Log($"Ammo: {current}/{max}");
    }
}
