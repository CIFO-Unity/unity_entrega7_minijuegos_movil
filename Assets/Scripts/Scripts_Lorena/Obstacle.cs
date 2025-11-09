using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float speed = 3f;
    public string poolTag = "Obstacle";
    public float despawnY = 11.6f;
     public bool playerDead = false;

    void Update()
    {
        if (playerDead) return;
         
        transform.Translate(Vector3.up * speed * Time.deltaTime);

        if (transform.position.y > despawnY)
        {
            ObjectPooler.Instance.ReturnToPool(poolTag, gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            var player = collision.collider.GetComponent<PlayerController>();
            if (player != null) player.Die();
        }
    }
}
