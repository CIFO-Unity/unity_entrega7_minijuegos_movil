using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Pool tags")]
    public string ringTag = "Ring";
    public string obstacleTag = "Obstacle";

    [Header("Spawn params")]
public float spawnInterval = 0.6f;
public float spawnXRange = 4.6f;
public float spawnYStart = -11.6f; 
public float obstacleSpeed = 3f;

[Header("Difficulty")]
public float intervalDecreaseRate = 0.04f; // se acelera más deprisa
public float minSpawnInterval = 0.25f;     // límite inferior más bajo


    void Start()
    {
        StartCoroutine(SpawnLoop());
        StartCoroutine(IncreaseDifficulty());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnPattern();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    IEnumerator IncreaseDifficulty()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f); // cada 5 segundos de juego
            spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval - intervalDecreaseRate);
        }
    }

    void SpawnPattern()
    {
        float r = Random.value;
        if (r < 0.6f) SpawnRing();
        else SpawnObstacle();

        if (Random.value < 0.15f)
        {
            if (Random.value < 0.5f) SpawnObstacle();
            else SpawnRing();
        }
    }

    void SpawnRing()
    {
        GameObject obj = ObjectPooler.Instance.GetFromPool(ringTag);
        if (obj == null) return;
        float x = Random.Range(-spawnXRange, spawnXRange);
        obj.transform.position = new Vector3(x, spawnYStart, 0f);
        obj.transform.rotation = Quaternion.identity;

        var ring = obj.GetComponent<Ring>();
        if (ring != null) ring.speed = obstacleSpeed;
    }

    void SpawnObstacle()
    {
        GameObject obj = ObjectPooler.Instance.GetFromPool(obstacleTag);
        if (obj == null) return;
        float x = Random.Range(-spawnXRange, spawnXRange);
        obj.transform.position = new Vector3(x, spawnYStart, 0f);
        obj.transform.rotation = Quaternion.identity;

        var ob = obj.GetComponent<Obstacle>();
        if (ob != null) ob.speed = obstacleSpeed;
    }
}
