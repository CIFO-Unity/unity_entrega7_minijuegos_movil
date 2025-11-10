using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Controla la UI del lobby:
/// - Muestra la lista de jugadores conectados
/// - Actualiza cuando el servidor envía LOBBY_UPDATE
/// - Inicia la transición a la escena de juego cuando recibe GAME_START
/// </summary>
public class LobbyUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [Tooltip("Panel que contiene la lista de jugadores")]
    public GameObject lobbyPanel;
    
    [Tooltip("Texto donde se muestra la lista de jugadores")]
    public TextMeshProUGUI playerListText;
    
    [Tooltip("Texto que muestra 'Esperando jugadores...' o 'Partida lista!'")]
    public TextMeshProUGUI statusText;
    
    [Header("Configuración")]
    [Tooltip("Nombre de la escena de gameplay")]
    public string gameplaySceneName = "Ender'sGame";

    private NetworkManager networkManager;

    void Start()
    {
        // Buscar el NetworkManager en la escena
        networkManager = FindObjectOfType<NetworkManager>();
        
        if (networkManager == null)
        {
            Debug.LogError("❌ LobbyUI no encuentra NetworkManager!");
            return;
        }

        // Suscribirse a los eventos del NetworkManager
        networkManager.OnLobbyUpdated += UpdateLobbyDisplay;
        networkManager.OnGameStart += StartGame;
        
        // Ocultar el panel del lobby al inicio
        if (lobbyPanel != null)
        {
            lobbyPanel.SetActive(false);
        }
        
        Debug.Log("✅ LobbyUI inicializado y escuchando eventos");
        
        // DEBUG: Verificar referencias
        Debug.Log($"🔍 lobbyPanel: {(lobbyPanel != null ? "OK" : "NULL")}");
        Debug.Log($"🔍 playerListText: {(playerListText != null ? "OK" : "NULL")}");
        Debug.Log($"🔍 statusText: {(statusText != null ? "OK" : "NULL")}");
        
        // TEST: Simular actualización de lobby (comentar después de probar)
        // TestLobbyDisplay(); // ✅ COMENTADO - UI funciona correctamente
    }
    
    // Método de prueba - puedes descomentar para probar la UI sin servidor
    void TestLobbyDisplay()
    {
        var testPlayers = new List<LobbyPlayer>
        {
            new LobbyPlayer { playerId = "1", name = "Felix", isHost = true },
            new LobbyPlayer { playerId = "2", name = "Juan", isHost = false }
        };
        UpdateLobbyDisplay(testPlayers, 2, 4);
    }

    void OnDestroy()
    {
        // Desuscribirse para evitar errores
        if (networkManager != null)
        {
            networkManager.OnLobbyUpdated -= UpdateLobbyDisplay;
            networkManager.OnGameStart -= StartGame;
        }
    }

    /// <summary>
    /// Llamado por el NetworkManager cuando recibe un mensaje LOBBY_UPDATE del servidor.
    /// Actualiza la lista visual de jugadores.
    /// </summary>
    /// <param name="players">Lista de jugadores en el lobby</param>
    /// <param name="currentPlayers">Número actual de jugadores</param>
    /// <param name="maxPlayers">Número máximo de jugadores</param>
    void UpdateLobbyDisplay(List<LobbyPlayer> players, int currentPlayers, int maxPlayers)
    {
        Debug.Log($"🔍 UpdateLobbyDisplay llamado con {players?.Count ?? 0} jugadores");
        
        // Mostrar el panel del lobby si estaba oculto
        if (lobbyPanel != null && !lobbyPanel.activeSelf)
        {
            lobbyPanel.SetActive(true);
            Debug.Log("✅ LobbyPanel activado");
        }

        // Construir el texto de la lista
        if (playerListText != null)
        {
            string listText = $"=== LOBBY ({currentPlayers}/{maxPlayers}) ===\n\n";
            
            if (players != null && players.Count > 0)
            {
                foreach (var player in players)
                {
                    // Mostrar símbolo de host
                    string hostIcon = player.isHost ? "👑 " : "   ";
                    listText += $"{hostIcon}{player.name}\n";
                }
            }
            else
            {
                listText += "Esperando jugadores...\n";
            }
            
            playerListText.text = listText;
            Debug.Log($"📝 PlayerListText actualizado:\n{listText}");
        }
        else
        {
            Debug.LogError("❌ playerListText es NULL!");
        }

        // Actualizar el mensaje de estado con conteo de jugadores
        if (statusText != null)
        {
            if (currentPlayers >= maxPlayers)
            {
                statusText.text = "✅ ¡Lobby completo! Iniciando partida...";
            }
            else
            {
                statusText.text = $"⏳ Esperando jugadores... ({currentPlayers}/{maxPlayers})";
            }
            Debug.Log($"📝 StatusText actualizado: {statusText.text}");
        }
        else
        {
            Debug.LogError("❌ statusText es NULL!");
        }

        Debug.Log($"📋 Lobby actualizado: {currentPlayers}/{maxPlayers} jugadores conectados");
    }

    /// <summary>
    /// Llamado por el NetworkManager cuando recibe un mensaje GAME_START del servidor.
    /// Inicia la transición a la escena de juego.
    /// </summary>
    void StartGame()
    {
        Debug.Log("🚀 Iniciando partida...");
        
        if (statusText != null)
        {
            statusText.text = "🎮 ¡Iniciando partida!";
        }

        // Cargar la escena de gameplay
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameplaySceneName);
    }
}
