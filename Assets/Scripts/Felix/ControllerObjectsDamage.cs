using UnityEngine;

public class ControllerObjectsDamage : MonoBehaviour
{
    [Header("Limits spawn objects")]
    [SerializeField]
    private GameObject spawnLeftLimits;
    [SerializeField]
    private GameObject spawnRightLimits;

    [Header("List of damage objects")]
    [SerializeField]
    private GameObject[] listGameObjectsDamage; // [0-2] = 3 asteroides, [3-5] = 3 naves enemigas
    //para saber el indice del array de cada lista de objetos de daño. Es para reiniciar a cero cuando se usan todos los objetos de daño de cada array.
    private int[] indexListGameObjectsDamage;

    private int listRandomDamage;
    private float positionRandomX;
    
    private NetworkManager networkManager;
    private bool isMultiplayer = false;
    private bool spawnEnabled = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        indexListGameObjectsDamage = new int[listGameObjectsDamage.Length];
        
        // Detectar si existe NetworkManager Y si está en una partida activa
        networkManager = FindFirstObjectByType<NetworkManager>();
        if (networkManager != null && networkManager.isGameStarted)
        {
            // NetworkManager existe Y el juego multijugador está activo
            isMultiplayer = true;
            spawnEnabled = true; // En multiplayer, enemigos controlados por servidor
            Debug.Log("🌐 NetworkManager detectado Y juego iniciado - Enemigos controlados por servidor");
        }
        else
        {
            // No hay NetworkManager activo = modo single player
            isMultiplayer = false;
            spawnEnabled = true;
            InvokeRepeating("CambiarPosicion", 2.0f, 2.0f);
            Debug.Log("🎮 ControllerObjectsDamage en modo SINGLE PLAYER - Spawning local activado");
        }
        
        print("Salva a la Tierra!!!!");
    }


    void Update()
    {
        // Si NetworkManager se activa después (caso raro), habilitar modo multiplayer
        if (!isMultiplayer && networkManager != null && networkManager.isGameStarted)
        {
            isMultiplayer = true;
            spawnEnabled = true;
            CancelInvoke("CambiarPosicion"); // Detener spawn local
            Debug.Log("🌐 NetworkManager activado durante el juego - Cambiando a modo multiplayer");
        }
    }

    private void CambiarPosicion()
    {
        positionRandomX = Random.Range(spawnLeftLimits.gameObject.transform.position.x, spawnRightLimits.gameObject.transform.position.x);
        listRandomDamage = Random.Range(0, listGameObjectsDamage.Length);
        listGameObjectsDamage[listRandomDamage].gameObject.transform.GetChild(indexListGameObjectsDamage[listRandomDamage]).transform.position = new Vector2(positionRandomX, spawnLeftLimits.gameObject.transform.position.y);
        listGameObjectsDamage[listRandomDamage].gameObject.transform.GetChild(indexListGameObjectsDamage[listRandomDamage]).GetComponent<Rigidbody2D>().gravityScale = 1.0f;
        listGameObjectsDamage[listRandomDamage].gameObject.transform.GetChild(indexListGameObjectsDamage[listRandomDamage]).GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0.0f, 0.0f);
        indexListGameObjectsDamage[listRandomDamage]++;
        //si es mayor o igual que el numero de hijos del gameobject, reiniciamos a cero el indice.
        if(indexListGameObjectsDamage[listRandomDamage] >= listGameObjectsDamage[listRandomDamage].gameObject.transform.childCount)
        {
            indexListGameObjectsDamage[listRandomDamage] = 0;
        }
    }
    
    /// <summary>
    /// Método público para spawnnear enemigos controlados por el servidor (multiplayer).
    /// </summary>
    public void SpawnEnemyFromServer(int enemyIndex, float posX, float posY, float velocityX, float velocityY)
    {
        // Solo spawnnear si el sistema está habilitado
        if (!spawnEnabled)
        {
            Debug.LogWarning("⚠️ SpawnEnemyFromServer llamado pero spawning no está habilitado aún");
            return;
        }
        
        // Validar que el índice sea válido
        if (enemyIndex < 0 || enemyIndex >= listGameObjectsDamage.Length)
        {
            Debug.LogWarning($"⚠️ Índice de enemigo inválido: {enemyIndex} (rango válido: 0-{listGameObjectsDamage.Length - 1})");
            return;
        }
        
        // Spawnnear el enemigo desde el pool usando el índice exacto del servidor
        GameObject enemyPool = listGameObjectsDamage[enemyIndex];
        
        // Validar que el pool tenga hijos disponibles
        if (enemyPool.transform.childCount == 0)
        {
            Debug.LogWarning($"⚠️ Pool de enemigos vacío para índice {enemyIndex}");
            return;
        }
        
        // Asegurar que el índice interno esté dentro del rango
        if (indexListGameObjectsDamage[enemyIndex] >= enemyPool.transform.childCount)
        {
            indexListGameObjectsDamage[enemyIndex] = 0;
        }
        
        Transform enemy = enemyPool.transform.GetChild(indexListGameObjectsDamage[enemyIndex]);
        
        enemy.position = new Vector2(posX, posY);
        enemy.GetComponent<Rigidbody2D>().gravityScale = 1.0f;
        enemy.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(velocityX, velocityY);
        
        indexListGameObjectsDamage[enemyIndex]++;
        if (indexListGameObjectsDamage[enemyIndex] >= enemyPool.transform.childCount)
        {
            indexListGameObjectsDamage[enemyIndex] = 0;
        }
        
        // Debug.Log($"👾 Enemigo spawneado desde servidor: índice={enemyIndex}, pos=({posX:F2}, {posY:F2}), vel=({velocityX:F2}, {velocityY:F2})");
    }

}
