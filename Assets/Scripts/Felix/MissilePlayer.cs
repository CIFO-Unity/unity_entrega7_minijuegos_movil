using UnityEngine;

public class MissilePlayer : MonoBehaviour
{

    [SerializeField]
    private GameObject explosionsMissiles;
    private int indexExplosionsArray;
    private int positionCementeryAllObjects;
    private int positionCementeryMissiles;
    private int incrementMissiles;//eje x para no estar uno encima de otro
    private int incrementObjectsEnemies;

    [SerializeField]
    private GameObject[] listSounds;

    [SerializeField]
    private GameObject accumulator;
    
    void Start()
    {
        positionCementeryAllObjects = 50;
        positionCementeryMissiles = 50;
        incrementMissiles = 5;
        incrementObjectsEnemies = 2;
        
        // Búsqueda automática de referencias
        if (explosionsMissiles == null)
        {
            explosionsMissiles = GameObject.Find("ExplosionsMissiles");
        }
        
        if (accumulator == null)
        {
            accumulator = GameObject.Find("Accumulator");
            if (accumulator == null)
            {
                accumulator = FindFirstObjectByType<UIController>()?.gameObject;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void HideExplosion()
    {
        for (int i = 0; i < explosionsMissiles.gameObject.transform.childCount; i++)
        {
            explosionsMissiles.gameObject.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    private bool hasScored = false; // Protección contra múltiples colisiones
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        // Verificar si es un enemigo o asteroide
        bool isEnemy = other.gameObject.CompareTag("Enemy1") || 
                       other.gameObject.CompareTag("Enemy2") || 
                       other.gameObject.CompareTag("Enemy3") || 
                       other.gameObject.CompareTag("Asteroid");
        
        if (!isEnemy) return;
        
        // Evitar múltiples puntuaciones por el mismo misil
        if (hasScored) return;
        hasScored = true;
        
        //print("Numero de veces que entro trigger.");
        
        // Añadir puntuación solo si accumulator existe
        if (accumulator != null)
        {
            var uiController = accumulator.GetComponent<UIController>();
            if (uiController != null)
            {
                uiController.increaseScore(10);
            }
        }
        
        // Reproducir sonido solo si existe
        if (listSounds != null && listSounds.Length > 0 && listSounds[0] != null)
        {
            listSounds[0].GetComponent<AudioSource>()?.Play();
        }
        
        // Mostrar explosión solo si existe
        if (explosionsMissiles != null && explosionsMissiles.transform.childCount > 0)
        {
            explosionsMissiles.transform.GetChild(indexExplosionsArray).transform.position = this.gameObject.transform.position;
            explosionsMissiles.transform.GetChild(indexExplosionsArray).gameObject.SetActive(true);

            Invoke("HideExplosion", 0.5f);

            indexExplosionsArray++;
            if (indexExplosionsArray >= explosionsMissiles.transform.childCount)
                indexExplosionsArray = 0;
        }

        //Damage Object (mover enemigo al cementerio)
        other.gameObject.transform.position = new Vector2(positionCementeryAllObjects, 8.0f);
        other.gameObject.GetComponent<Rigidbody2D>().gravityScale = 0.0f;
        other.gameObject.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0.0f, 0.0f);
        //Missile (mover misil al cementerio)
        this.gameObject.transform.position = new Vector2(positionCementeryMissiles, 10.0f);
        this.gameObject.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0.0f, 0.0f);
        positionCementeryAllObjects += incrementObjectsEnemies;
        positionCementeryMissiles += incrementMissiles;
        
        hasScored = false; // Resetear para que el misil pueda ser reutilizado

    }
}
