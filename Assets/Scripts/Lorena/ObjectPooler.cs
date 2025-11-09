using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    [Header("Pool Settings")]
    public float poolY = -15f; // Posición Y donde están los objetos en espera
    public float spawnY = -11.6f; // Posición Y donde aparecen para subir

    private List<Ring> ringPool = new List<Ring>();
    private List<Obstacle> obstaclePool = new List<Obstacle>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitializePool();
    }

    void InitializePool()
    {
        // Encontrar todos los Rings y Obstacles en la escena
        ringPool = FindObjectsByType<Ring>(FindObjectsSortMode.None).ToList();
        obstaclePool = FindObjectsByType<Obstacle>(FindObjectsSortMode.None).ToList();

        // Colocar todos en la zona de pool (fuera de pantalla)
        foreach (Ring ring in ringPool)
        {
            ResetToPool(ring.gameObject);
        }

        foreach (Obstacle obstacle in obstaclePool)
        {
            ResetToPool(obstacle.gameObject);
        }

        //Debug.Log($"Pool inicializado: {ringPool.Count} Rings, {obstaclePool.Count} Obstacles");
    }

    public GameObject GetRingFromPool()
    {
        // Buscar un Ring que esté en la zona de pool
        Ring availableRing = ringPool.FirstOrDefault(r => r.transform.position.y <= poolY);
        
        if (availableRing != null)
        {
            return availableRing.gameObject;
        }

        //Debug.LogWarning("No hay Rings disponibles en el pool");
        return null;
    }

    public GameObject GetObstacleFromPool()
    {
        // Buscar un Obstacle que esté en la zona de pool
        Obstacle availableObstacle = obstaclePool.FirstOrDefault(o => o.transform.position.y <= poolY);
        
        if (availableObstacle != null)
        {
            return availableObstacle.gameObject;
        }

        //Debug.LogWarning("No hay Obstacles disponibles en el pool");
        return null;
    }

    public void ReturnToPool(GameObject obj)
    {
        ResetToPool(obj);
    }

    void ResetToPool(GameObject obj)
    {
        // Colocar el objeto en la zona de pool, centrado en X
        obj.transform.position = new Vector3(0f, poolY, 0f);
    }

    public Vector3 GetSpawnPosition(float randomX)
    {
        return new Vector3(randomX, spawnY, 0f);
    }
}