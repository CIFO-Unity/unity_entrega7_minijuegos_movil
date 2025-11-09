using UnityEngine;

public class Ring : MonoBehaviour
{
    public float speed = 3f;
    bool scored = false;
    public float despawnY = 11.6f;
    public bool playerDead = false;

    void Update()
    {
        if (playerDead) return;

        // Solo moverse si está en la zona de juego (por encima del pool)
        if (transform.position.y > ObjectPooler.Instance.poolY)
        {
            transform.Translate(Vector3.up * speed * Time.deltaTime);

            if (transform.position.y > despawnY)
            {
                ReturnToPool();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (scored) return;
        if (other.CompareTag("Player"))
        {
            scored = true;
            GameManager.Instance.AddScore(1);
            // Opcional: play VFX/SFX
            //ReturnToPool();
        }
    }

    void ReturnToPool()
    {
        scored = false;
        ObjectPooler.Instance.ReturnToPool(gameObject);
    }
}
