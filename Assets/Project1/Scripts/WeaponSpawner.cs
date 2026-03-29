using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class WeaponSpawner : NetworkBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject weaponPrefab;
    [SerializeField] private List<Transform> spawnPositions = new List<Transform>();
    [SerializeField] private float spawnInterval = 5f;
    
    [Header("Optional Settings")]
    [SerializeField] private int maxWeaponsInScene = 5;
    [SerializeField] private bool spawnOnStart = true;
    
    private List<GameObject> spawnedWeapons = new List<GameObject>();
    private Coroutine spawnCoroutine;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        

        if (IsServer)
        {
            if (spawnOnStart)
            {

                SpawnWeapon();
            }
            
            spawnCoroutine = StartCoroutine(SpawnRoutine());
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            spawnedWeapons.RemoveAll(weapon => weapon == null);
            
            if (spawnedWeapons.Count < maxWeaponsInScene)
            {
                SpawnWeapon();
            }
        }
    }

    private void SpawnWeapon()
    {
        if (!IsServer) return;
        
    
        if (spawnPositions.Count == 0)
        {
            return;
        }
        
        Transform spawnPoint = spawnPositions[Random.Range(0, spawnPositions.Count)];
        
        GameObject weapon = Instantiate(weaponPrefab, spawnPoint.position, spawnPoint.rotation);
        
        //all clients see it
        NetworkObject networkObject = weapon.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
            spawnedWeapons.Add(weapon);
        }
        else
        {
            Destroy(weapon);
        }
    }
    
}