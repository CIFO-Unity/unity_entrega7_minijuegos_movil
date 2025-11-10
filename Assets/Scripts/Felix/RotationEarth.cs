using System;
using UnityEngine;

public class RotateEarth : MonoBehaviour
{
    // Velocidad de rotación (grados por segundo)
    [SerializeField]
    [Range(1f, 20f)]
    public float rotationSpeed = 3f;

    void Update()
    {
        // Rotar en el sentido de las agujas del reloj (alrededor del eje Z)
        transform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
    }
}
