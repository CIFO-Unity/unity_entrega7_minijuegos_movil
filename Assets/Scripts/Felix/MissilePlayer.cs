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
        
        // Ignorar colisiones con otros objetos (jugador, otros misiles, etc.)
        if (!isEnemy) return;
        
        // Evitar múltiples puntuaciones por el mismo misil
        if (hasScored) return;
        hasScored = true;
        
        //print("Numero de veces que entro trigger.");
        accumulator.gameObject.GetComponent<UIController>().increaseScore(10);
        listSounds[0].gameObject.GetComponent<AudioSource>().Play();
        explosionsMissiles.gameObject.transform.GetChild(indexExplosionsArray).transform.position = this.gameObject.transform.position;
        explosionsMissiles.gameObject.transform.GetChild(indexExplosionsArray).gameObject.SetActive(true);

        Invoke("HideExplosion", 0.5f);

        indexExplosionsArray++;
        if (indexExplosionsArray >= explosionsMissiles.transform.childCount)
            indexExplosionsArray = 0;

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
