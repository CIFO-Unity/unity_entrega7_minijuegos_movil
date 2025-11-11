using UnityEngine;

public class GhostShipCollision : MonoBehaviour
{
    [Header("Sistema de Explosiones")]
    [Tooltip("GameObject padre que contiene las 3 explosiones como hijos (se busca automáticamente si no se asigna)")]
    public GameObject explosions;
    
    [Header("Audio (Opcional)")]
    [Tooltip("Sonido de explosión")]
    public AudioClip explosionSound;

    private int indexExplosionsArray = 0;

    void Start()
    {
        // Buscar automáticamente el GameObject de explosiones en la escena
        if (explosions == null)
        {
            explosions = GameObject.Find("ExplosionsMissiles");
            
            if (explosions == null)
            {
                GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name.Contains("Explosion") && obj.transform.childCount >= 3)
                    {
                        explosions = obj;
                        break;
                    }
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // IGNORAR colisiones con naves (locales y fantasmas) y misiles
        if (collision.gameObject.CompareTag("Player") || 
            collision.gameObject.CompareTag("Albert") ||
            collision.gameObject.name.Contains("Missile"))
        {
            return; // No hacer nada con estas colisiones
        }
        
        // Solo destruir asteroides y enemigos
        if (collision.gameObject.CompareTag("Asteroid") || 
            collision.gameObject.CompareTag("Enemy1") || 
            collision.gameObject.CompareTag("Enemy2") || 
            collision.gameObject.CompareTag("Enemy3"))
        {
            DestroyObject(collision.gameObject);
        }
    }

    /// <summary>
    /// Desactiva el objeto (sistema de pooling) con efectos opcionales
    /// </summary>
    void DestroyObject(GameObject obj)
    {
        // Mostrar explosión si está configurada
        if (explosions != null && explosions.transform.childCount > 0)
        {
            // Posicionar y activar la explosión actual
            explosions.transform.GetChild(indexExplosionsArray).transform.position = obj.transform.position;
            explosions.transform.GetChild(indexExplosionsArray).gameObject.SetActive(true);

            // Ocultar la explosión después de 0.5 segundos
            StartCoroutine(HideExplosionAfterDelay(indexExplosionsArray, 0.5f));

            // Avanzar al siguiente índice de explosión (sistema cíclico)
            indexExplosionsArray++;
            if (indexExplosionsArray >= explosions.transform.childCount)
                indexExplosionsArray = 0;
        }

        // Reproducir sonido si está configurado
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, obj.transform.position);
        }

        // NO DESTRUIR: Mover al "cementerio" para reutilizar (sistema de pooling)
        obj.transform.position = new Vector2(50f, 8f);
        var rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0.0f;
            rb.linearVelocity = Vector2.zero;
        }
    }

    /// <summary>
    /// Corrutina para ocultar la explosión después de un tiempo
    /// </summary>
    private System.Collections.IEnumerator HideExplosionAfterDelay(int explosionIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (explosions != null && explosionIndex < explosions.transform.childCount)
        {
            explosions.transform.GetChild(explosionIndex).gameObject.SetActive(false);
        }
    }
}
