using UnityEngine;

public class GhostShipCollision : MonoBehaviour
{
    [Header("Efectos (Opcional)")]
    [Tooltip("Prefab de explosión a instanciar")]
    public GameObject explosionPrefab;
    
    [Tooltip("Sonido de explosión")]
    public AudioClip explosionSound;

    void OnCollisionEnter2D(Collision2D collision)
    {
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
    /// Destruye el objeto con efectos opcionales
    /// </summary>
    void DestroyObject(GameObject obj)
    {
        // Instanciar explosión si está configurada
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, obj.transform.position, Quaternion.identity);
        }

        // Reproducir sonido si está configurado
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, obj.transform.position);
        }

        // Destruir el objeto
        Destroy(obj);
    }
}
