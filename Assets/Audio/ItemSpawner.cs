using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public float spawnInterval = 2f;
    public float minX = -8f;
    public float maxX = 8f;

    [Header("Spawn Probability")]
    [Range(0f, 1f)]
    public float ringProbability = 0.5f; // 50% Ring, 50% Obstacle

    private float nextSpawnTime;
    public bool playerDead = false;

    void Start()
    {
        nextSpawnTime = Time.time + spawnInterval;
    }

    void Update()
    {
        if (playerDead) return;

        if (Time.time >= nextSpawnTime)
        {
            SpawnItem();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnItem()
    {
        // Decidir si spawn Ring u Obstacle
        bool spawnRing = Random.value <= ringProbability;
        
        GameObject itemToSpawn = spawnRing 
            ? ObjectPooler.Instance.GetRingFromPool() 
            : ObjectPooler.Instance.GetObstacleFromPool();

        if (itemToSpawn == null)
        {
            Debug.LogWarning("No hay objetos disponibles en el pool");
            return;
        }

        // Posición aleatoria en X
        float randomX = Random.Range(minX, maxX);
        Vector3 spawnPosition = ObjectPooler.Instance.GetSpawnPosition(randomX);
        
        // Mover el objeto a la posición de spawn
        itemToSpawn.transform.position = spawnPosition;

        // Propagar el estado de playerDead
        Ring ring = itemToSpawn.GetComponent<Ring>();
        if (ring != null)
        {
            ring.playerDead = playerDead;
        }

        Obstacle obstacle = itemToSpawn.GetComponent<Obstacle>();
        if (obstacle != null)
        {
            obstacle.playerDead = playerDead;
        }
    }

    public void StopSpawning()
    {
        playerDead = true;
    }
}