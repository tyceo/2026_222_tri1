using UnityEngine;
using Unity.Netcode;

public class CubeSpawner : NetworkBehaviour
{
    [Header("Cube Settings")]
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private float cubeSize = 1f;
    
    void Update()
    {
        //only the server/host can spawn objects
        if (!IsServer) return;
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnCubes(100);
        }
    }
    
    private void SpawnCubes(int count)
    {
        for (int i = 0; i < count; i++)
        {

            Vector3 randomPosition = transform.position + Random.insideUnitSphere * spawnRadius;
            

            GameObject cube = Instantiate(cubePrefab, randomPosition, Random.rotation);
            cube.transform.localScale = Vector3.one * cubeSize;
            

            NetworkObject networkObject = cube.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Spawn();
            }
        }
        
    }
}