using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFixedWidth : MonoBehaviour
{
    #region Variables

    [SerializeField]
    private float targetWorldWidth = 5f;       // Ancho visible deseado en unidades del mundo

    private Camera cam;                        // Referencia al componente Camera
    private float lastAspect;                  // Último aspect ratio calculado para detectar cambios

    #endregion

    #region Start & Update

    // Inicializar componentes y ajustar la cámara al comenzar
    void Start()
    {
        // Obtener el componente Camera del GameObject
        cam = GetComponent<Camera>();
        // Realizar el primer ajuste de la cámara
        AdjustCamera();
    }

    // Verificar cambios en el aspect ratio cada frame
    void Update()
    {
        // Calcular el aspect ratio actual de la pantalla
        float currentAspect = (float)Screen.width / Screen.height;
        
        // Verificar si el aspect ratio ha cambiado (rotación, cambio de resolución, etc.)
        if (Mathf.Abs(currentAspect - lastAspect) > 0.001f)
        {
            // Reajustar la cámara con el nuevo aspect ratio
            AdjustCamera();
        }
    }

    #endregion

    #region Ajuste de cámara

    // Ajustar el tamaño ortográfico de la cámara para mantener el ancho constante
    void AdjustCamera()
    {
        // Calcular el aspect ratio actual
        float aspect = (float)Screen.width / Screen.height;
        
        // Calcular el orthographicSize que mantiene el ancho constante
        // Fórmula: orthographicSize = targetWorldWidth / (2 * aspect)
        cam.orthographicSize = targetWorldWidth / (2f * aspect);
        
        // Guardar el aspect ratio para detectar futuros cambios
        lastAspect = aspect;
    }

    #endregion
}