using UnityEngine;

public class UI_PopIn : MonoBehaviour
{
    public float duration = 0.5f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        StartCoroutine(Animate());
    }

    System.Collections.IEnumerator Animate()
    {
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = curve.Evaluate(t / duration);
            transform.localScale = Vector3.LerpUnclamped(Vector3.zero, originalScale, progress);
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
