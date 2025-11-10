using UnityEngine;

/// <summary>
/// Configuración automática para modo ventana en builds
/// Esto permite tener múltiples instancias visibles simultáneamente
/// CRÍTICO: Habilita "Run In Background" para multiplayer
/// </summary>
public class WindowedModeConfig : MonoBehaviour
{
    [Header("Configuración de Ventana")]
    [Tooltip("Ancho de la ventana en píxeles")]
    [SerializeField] private int windowWidth = 960;
    
    [Tooltip("Alto de la ventana en píxeles")]
    [SerializeField] private int windowHeight = 540;
    
    [Tooltip("¿Ejecutar en modo ventana?")]
    [SerializeField] private bool windowedMode = true;
    
    [Tooltip("¿Permitir redimensionar la ventana?")]
    [SerializeField] private bool resizable = true;

    void Awake()
    {
        // ⚠️ CRÍTICO PARA MULTIPLAYER: Ejecutar en segundo plano
        Application.runInBackground = true;
        Debug.Log("✅ Run In Background HABILITADO - El juego seguirá ejecutándose sin foco");
        
        // Solo aplicar en builds (no en el editor)
        #if !UNITY_EDITOR
        if (windowedMode)
        {
            // Configurar modo ventana
            Screen.SetResolution(windowWidth, windowHeight, FullScreenMode.Windowed);
            Debug.Log($"🪟 Modo ventana configurado: {windowWidth}x{windowHeight}");
        }
        #else
        Debug.Log("⚙️ WindowedModeConfig: Modo ventana solo funciona en builds");
        #endif
    }

    // Método para cambiar el tamaño en tiempo de ejecución
    public void SetWindowSize(int width, int height)
    {
        Screen.SetResolution(width, height, FullScreenMode.Windowed);
        Debug.Log($"🪟 Ventana redimensionada a: {width}x{height}");
    }

    // Alternar entre ventana y pantalla completa
    public void ToggleFullscreen()
    {
        if (Screen.fullScreenMode == FullScreenMode.Windowed)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Debug.Log("🖥️ Cambiado a pantalla completa");
        }
        else
        {
            Screen.SetResolution(windowWidth, windowHeight, FullScreenMode.Windowed);
            Debug.Log("🪟 Cambiado a modo ventana");
        }
    }
}
