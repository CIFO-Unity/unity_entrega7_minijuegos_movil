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
        // Si no se asignó manualmente, buscar en la escena
        if (explosions == null)
        {
            explosions = GameObject.Find("Explosions");
            
            if (explosions == null)
            {
                Debug.LogWarning("⚠️ GhostShipCollision: No se encontró el GameObject 'Explosions' en la escena");
            }
        }
    }

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

        // Destruir el objeto
        Destroy(obj);
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
