using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public float minX = -8f;
    public float maxX = 8f;

    [Header("Difficulty Progression")]
    public float initialSpawnInterval = 3f; // Intervalo inicial (más lento)
    public float minSpawnInterval = 0.8f; // Intervalo mínimo (más rápido)
    public float difficultyIncreaseRate = 0.05f; // Cuánto disminuye el intervalo por segundo
    
    [Header("Spawn Probability")]
    [Range(0f, 1f)]
    public float ringProbability = 0.5f; // 50% Ring, 50% Obstacle

    private float currentSpawnInterval;
    private float nextSpawnTime;
    private float gameTime;
    public bool playerDead = false;

    void Start()
    {
        currentSpawnInterval = initialSpawnInterval;
        nextSpawnTime = Time.time + currentSpawnInterval;
        gameTime = 0f;
    }

    void Update()
    {
        if (playerDead) return;

        // Aumentar dificultad con el tiempo
        gameTime += Time.deltaTime;
        UpdateDifficulty();

        if (Time.time >= nextSpawnTime)
        {
            SpawnItem();
            nextSpawnTime = Time.time + currentSpawnInterval;
        }
    }

    void UpdateDifficulty()
    {
        // Disminuir el intervalo de spawn progresivamente
        currentSpawnInterval = Mathf.Max(
            minSpawnInterval, 
            initialSpawnInterval - (gameTime * difficultyIncreaseRate)
        );
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

    // Método opcional para obtener info de dificultad (útil para UI)
    public float GetCurrentDifficulty()
    {
        return 1f - (currentSpawnInterval - minSpawnInterval) / (initialSpawnInterval - minSpawnInterval);
    }

    public float GetCurrentSpawnInterval()
    {
        return currentSpawnInterval;
    }
}