using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float smoothTime = 0.06f;

    [Header("Bounds")]
    public float leftLimit = -4.8f;
    public float rightLimit = 4.8f;

    [Header("Refs")]
    public SpriteRenderer spriteRenderer;   // assign in inspector (or will GetComponent)
    public Sprite dieSprite;                // optional: sprite to show on death

    Camera mainCam;
    Vector3 velocity = Vector3.zero;
    bool isAlive = true;

    // small threshold to ignore tiny movements (prevents rapid flip jitter)
    const float flipThreshold = 0.02f;

    void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        mainCam = Camera.main;
    }

    void Update()
    {
        if (!isAlive) return;
        HandleTouchMovement();
    }

    void HandleTouchMovement()
    {
        Vector3 targetWorldPos = transform.position;

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            Vector3 screenPoint = new Vector3(t.position.x, t.position.y, Mathf.Abs(mainCam.transform.position.z));
            targetWorldPos = mainCam.ScreenToWorldPoint(screenPoint);
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 screenPoint = Input.mousePosition;
            screenPoint.z = Mathf.Abs(mainCam.transform.position.z);
            targetWorldPos = mainCam.ScreenToWorldPoint(screenPoint);
        }
        else
        {
            float h = Input.GetAxis("Horizontal");
            if (Mathf.Abs(h) > 0.01f)
                targetWorldPos += new Vector3(h * moveSpeed * Time.deltaTime, 0f, 0f);
        }

        float targetX = Mathf.Clamp(targetWorldPos.x, leftLimit, rightLimit);
        Vector3 desiredPos = new Vector3(targetX, transform.position.y, transform.position.z);

        // Decide flip based on horizontal delta (target vs current)
        float deltaX = targetX - transform.position.x;
        if (deltaX > flipThreshold)
        {
            spriteRenderer.flipX = false; // facing right (default)
        }
        else if (deltaX < -flipThreshold)
        {
            spriteRenderer.flipX = true;  // facing left
        }
        // If deltaX is very small, keep previous flipX (no change)

        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, smoothTime, moveSpeed);
    }

    public void Die()
    {
        if (!isAlive) return;
        isAlive = false;

        // If you provided a die sprite, use it; otherwise keep current sprite
        if (dieSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = dieSprite;
        }

        // Notify GameManager
        GameManager.Instance.OnPlayerDeath();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAlive) return;
        if (other.CompareTag("Obstacle"))
        {
            Die();
        }
    }
}
