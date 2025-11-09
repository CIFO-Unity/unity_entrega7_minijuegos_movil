using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float speed = 3f;
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
                ObjectPooler.Instance.ReturnToPool(gameObject);
            }
        }
    }
}