using UnityEngine;

public class Ring : MonoBehaviour
{
    public float speed = 3f;
    public string poolTag = "Ring";
    bool scored = false;
    public float despawnY = 11.6f;

    void OnEnable()
    {
        scored = false;
    }

    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);

        if (transform.position.y > despawnY)
        {
            ReturnToPool();
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
            ReturnToPool();
        }
    }

    void ReturnToPool()
    {
        ObjectPooler.Instance.ReturnToPool(poolTag, gameObject);
    }
}
