using UnityEngine;
using System.Collections;

public class TimedSnapRotator : MonoBehaviour
{
    [Header("Configuración de rotación")]
    public Vector3 rotationOffset = new Vector3(0f, 0f, 15f); // cuánto rota de golpe
    public float rotateInterval = 2f; // cada cuántos segundos rota
    public float stayRotatedTime = 1f; // cuánto tiempo permanece rotado

    private Quaternion originalRotation;

    void Start()
    {
        originalRotation = transform.rotation;
        StartCoroutine(RotationLoop());
    }

    IEnumerator RotationLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(rotateInterval);

            // Rota de golpe
            transform.rotation = Quaternion.Euler(originalRotation.eulerAngles + rotationOffset);

            yield return new WaitForSeconds(stayRotatedTime);

            // Vuelve a la rotación original
            transform.rotation = originalRotation;
        }
    }
}
