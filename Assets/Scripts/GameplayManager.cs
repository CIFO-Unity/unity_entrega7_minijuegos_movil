using UnityEngine;
using UnityEngine.SceneManagement;

// Script que se ejecuta en la escena de gameplay (EndersGame)
// Lee la elección del jugador y configura el NetworkManager según corresponda
public class GameplayManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Player2D player2D; // Referencia al Player2D

    private NetworkManager networkManager; // Se busca automáticamente

    // Clave para leer el modo de juego (misma que en GameModeSelector)
    private const string GAME_MODE_KEY = "GameMode";

    void Start()
    {
        // Buscar NetworkManager (puede estar en DontDestroyOnLoad)
        networkManager = FindObjectOfType<NetworkManager>();
        
        ConfigureGameMode();
    }

    // ------------------------------------------------------
    // Configurar el modo de juego según la elección del menú
    void ConfigureGameMode()
    {
        // Si hay NetworkManager y está en una partida, es modo Multiplayer
        if (networkManager != null && networkManager.isGameStarted)
        {
            Debug.Log("🌐 Entrando en modo Multiplayer (lobby detectado)...");
            ConfigureMultiplayerMode();
        }
        else
        {
            // Si no hay NetworkManager o no hay partida activa, es SinglePlayer
            Debug.Log("🎯 Entrando en modo Single Player...");
            ConfigureSinglePlayerMode();
        }

        // Iniciar el Player2D en cualquier caso
        if (player2D != null)
        {
            player2D.StartGame();
            Debug.Log("🚀 Player2D iniciado");
        }
        else
        {
            Debug.LogWarning("⚠️ Player2D no asignado en el Inspector!");
        }
    }

    // ------------------------------------------------------
    // Configurar modo multijugador
    void ConfigureMultiplayerMode()
    {
        Debug.Log("🔍 ConfigureMultiplayerMode() ejecutándose...");
        
        // Buscar el NetworkManager que persistió desde Felix (con DontDestroyOnLoad)
        if (networkManager == null)
        {
            networkManager = FindObjectOfType<NetworkManager>();
            Debug.Log($"🔍 NetworkManager encontrado: {(networkManager != null ? "SÍ" : "NO")}");
        }
        
        if (networkManager != null)
        {
            // Verificar si el GameObject está activo
            Debug.Log($"🔍 GameObject activo: {networkManager.gameObject.activeInHierarchy}");
            Debug.Log($"🔍 Componente enabled antes: {networkManager.enabled}");
            
            // PRIMERO: Activar el GameObject si está desactivado
            if (!networkManager.gameObject.activeInHierarchy)
            {
                Debug.Log("🔋 ACTIVANDO GameObject NetworkManager...");
                networkManager.gameObject.SetActive(true);
                Debug.Log("✅ GameObject NetworkManager ACTIVADO");
            }
            
            // SEGUNDO: Activar el componente (ya debería estar activo si el GameObject se activó)
            if (!networkManager.enabled)
            {
                networkManager.enabled = true;
                Debug.Log("✅ Componente NetworkManager HABILITADO");
            }
            
            // TERCERO: Iniciar envío de posición al servidor
            Debug.Log("📡 Iniciando envío de posición al servidor...");
            networkManager.StartSendingPosition();
            
            Debug.Log($"📊 Estado final - GameObject: {networkManager.gameObject.activeInHierarchy}, Componente: {networkManager.enabled}");
        }
        else
        {
            Debug.LogError("❌ NetworkManager no encontrado. ¿Llegaste desde la escena Felix?");
        }
    }

    // ------------------------------------------------------
    // Configurar modo single player
    void ConfigureSinglePlayerMode()
    {
        if (networkManager != null)
        {
            // Desactivar el GameObject completamente para Single Player
            if (networkManager.gameObject.activeInHierarchy)
            {
                networkManager.gameObject.SetActive(false);
                Debug.Log("🚫 GameObject NetworkManager DESACTIVADO - Modo single player");
            }
        }
        
        Debug.Log("✅ Modo single player configurado");
    }

    // ------------------------------------------------------
    // Método público para volver al menú (puede ser llamado desde UI)
    public void ReturnToMenu()
    {
        // Desactivar NetworkManager si está activo
        if (networkManager != null && networkManager.enabled)
        {
            networkManager.enabled = false;
            Debug.Log("🔌 NetworkManager desactivado");
        }

        // Detener el Player2D
        if (player2D != null)
        {
            player2D.StopGame();
            Debug.Log("⏸️ Player2D detenido");
        }

        // Volver al menú de selección
        SceneManager.LoadScene("Felix");
    }

    // ------------------------------------------------------
    // Método para debugging
    [ContextMenu("Show Current Game Mode")]
    void ShowCurrentGameMode()
    {
        string mode = PlayerPrefs.GetString(GAME_MODE_KEY, "SinglePlayer");
        Debug.Log($"🔍 Modo actual: {mode}");
    }
}