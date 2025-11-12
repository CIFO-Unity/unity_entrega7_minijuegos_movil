using UnityEngine;
using TMPro;

/// <summary>
/// Gestiona la UI del lobby en la escena Felix.
/// Maneja los botones de crear/unirse a partida y valida el número de jugadores.
/// </summary>
public class LobbyManager : MonoBehaviour
{
    [Header("Referencias UI")]
    [Tooltip("InputField donde el usuario escribe el número de jugadores (2-4)")]
    public TMP_InputField maxPlayersInput;
    
    [Tooltip("Referencia al NetworkManager (se busca automáticamente si no se asigna)")]
    public NetworkManager networkManager;
    [Header("Referencias UI (opcional)")]
    [Tooltip("Referencia al componente LobbyUI. Si se deja vacío, se buscará automáticamente en la escena.")]
    public LobbyUI lobbyUI;
    [Tooltip("Panel del lobby (GameObject). Si se asigna aquí, se activará directamente sin depender de LobbyUI).")]
    public GameObject lobbyPanel;

    void Start()
    {
        // Buscar NetworkManager si no está asignado
        if (networkManager == null)
        {
            networkManager = FindFirstObjectByType<NetworkManager>();
            
            if (networkManager == null)
            {
                Debug.LogError("❌ LobbyManager no encuentra NetworkManager!");
            }
        }

        // Valor por defecto en el input: 2 jugadores
        if (maxPlayersInput != null)
        {
            maxPlayersInput.text = "2";
        }
            // Si no se asignó en el Inspector, cachear referencia a LobbyUI (usar API no obsoleta)
            if (lobbyUI == null)
            {
                lobbyUI = FindFirstObjectByType<LobbyUI>();
                if (lobbyUI == null)
                {
                    Debug.Log("⚠️ LobbyUI no encontrado en la escena (se activará cuando exista)");
                }
                else
                {
                    Debug.Log($"✅ LobbyUI encontrado automáticamente: {lobbyUI.name}");
                }
            }
    }

    /// <summary>
    /// Llamado por el botón "Crear Partida".
    /// Lee el número de jugadores del InputField y crea la partida.
    /// </summary>
    public void OnCreateGameClicked()
    {
        if (networkManager == null)
        {
            Debug.LogError("❌ NetworkManager no disponible");
            return;
        }

        if (maxPlayersInput == null)
        {
            Debug.LogError("❌ maxPlayersInput no asignado");
            return;
        }

        // Leer y validar el número de jugadores
        string inputText = maxPlayersInput.text.Trim();
        
        if (string.IsNullOrEmpty(inputText))
        {
            Debug.LogWarning("⚠️ Por favor, ingresa el número de jugadores");
            maxPlayersInput.text = "4"; // Valor por defecto
            return;
        }

        if (!int.TryParse(inputText, out int maxPlayers))
        {
            Debug.LogWarning("⚠️ Número de jugadores inválido. Debe ser un número entre 2 y 4");
            maxPlayersInput.text = "4";
            return;
        }

        // Validar rango (2-4 jugadores)
        if (maxPlayers < 2 || maxPlayers > 4)
        {
            Debug.LogWarning($"⚠️ Número de jugadores debe ser entre 2 y 4. Ingresaste: {maxPlayers}");
            maxPlayersInput.text = "4";
            return;
        }

        // Todo OK, crear partida
        Debug.Log($"🎯 Creando partida para {maxPlayers} jugadores...");
        networkManager.CreateGame(maxPlayers);
        // Mostrar inmediatamente el panel del lobby en modo "esperando"
        // Preferir activar el panel directo si está asignado en este componente
        if (lobbyPanel != null)
        {
            lobbyPanel.SetActive(true);
        }
        else
        {
            if (lobbyUI == null)
            {
                lobbyUI = FindFirstObjectByType<LobbyUI>();
            }
            if (lobbyUI != null)
            {
                lobbyUI.ShowWaitingLobby(maxPlayers);
            }
        }
    }

    /// <summary>
    /// Llamado por el botón "Unirse a Partida".
    /// No necesita el número de jugadores (lo define el host).
    /// </summary>
    public void OnJoinGameClicked()
    {
        if (networkManager == null)
        {
            Debug.LogError("❌ NetworkManager no disponible");
            return;
        }

        Debug.Log("🔗 Uniéndose a partida...");
        networkManager.JoinGame();
        // Mostrar inmediatamente el panel de lobby en modo "esperando" (sin conocer maxPlayers)
        // Preferir activar el panel directo si está asignado en este componente
        if (lobbyPanel != null)
        {
            lobbyPanel.SetActive(true);
        }
        else
        {
            if (lobbyUI == null)
            {
                lobbyUI = FindFirstObjectByType<LobbyUI>();
            }
            if (lobbyUI != null)
            {
                lobbyUI.ShowWaitingLobby();
            }
        }
    }
}
