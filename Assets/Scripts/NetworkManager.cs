using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

/// <summary>
/// Cliente UDP para conectarse al servidor de juego Java.
/// Gestiona el lobby multijugador (2-4 jugadores) y el juego en tiempo real.
/// </summary>
public class NetworkManager : MonoBehaviour
{
    // ========== CONFIGURACIÓN DE RED ==========
    [Header("Configuración de red")]
    public string serverIp = "161.22.45.17"; //"127.0.0.1"; // IP del servidor (localhost para pruebas)
    public int serverPort = 5560;         // Puerto del servidor

    // ========== PREFABS ==========
    [Header("Prefabs de jugador")]
    public GameObject otherPlayerPrefab;   // Prefab para crear jugadores remotos

    [Header("Prefabs de gameplay (enemigos y balas)")]
    public GameObject asteroidPrefab;      // Prefab para asteroides
    public GameObject missileEnemyPrefab;  // Prefab para naves enemigas
    // bulletPrefab ya no es necesario - usamos el pool de Player2D

    // ========== REFERENCIAS ==========
    [Header("Referencia al Player2D (se busca automáticamente en gameplay)")]
    [SerializeField]
    private GameObject localPlayer2D;      // Tu nave (se asigna automáticamente en Ender'sGame)

    // ========== VARIABLES DE RED ==========
    private UdpClient udpClient;           // Cliente UDP para comunicación con servidor
    private IPEndPoint serverEndPoint;     // Dirección del servidor
    private Thread recvThread;             // Hilo para recibir mensajes del servidor

    /// <summary>
    /// ID del jugador asignado por el servidor.
    /// El servidor responde con un ID único tipo "player_123"
    /// </summary>
    private string playerId = "auto";

    /// <summary>
    /// Estado del lobby/juego
    /// </summary>
    private bool isInLobby = false;          // true = en lobby esperando, false = jugando
    public bool isGameStarted { get; private set; } = false;      // true cuando el juego ha comenzado
    private List<LobbyPlayer> lobbyPlayers = new List<LobbyPlayer>();  // Jugadores en lobby
    private int currentMaxPlayers = 4;       // Número máximo de jugadores (se actualiza con LOBBY_UPDATE)
    
    /// <summary>
    /// Eventos para actualizar la UI del lobby
    /// </summary>
    public System.Action<List<LobbyPlayer>, int, int> OnLobbyUpdated;  // (jugadores, actual, máximo)
    public System.Action OnGameStart;                                    // Se dispara al recibir GAME_START

    /// <summary>
    /// Diccionario que almacena todos los GameObjects de jugadores REMOTOS.
    /// Clave: playerId (ej: "player_123")
    /// Valor: GameObject de la nave del otro jugador
    /// </summary>
    private readonly Dictionary<string, GameObject> players = new();
    
    /// <summary>
    /// Diccionario de enemigos remotos (sincronizados por el servidor)
    /// Clave: enemyId (ej: "enemy_456")
    /// Valor: GameObject del enemigo
    /// </summary>
    private readonly Dictionary<string, GameObject> remoteEnemies = new();
    
    // Ya no necesitamos remoteBullets - usamos el pool de misiles de Player2D
    
    /// <summary>
    /// Cola de mensajes recibidos del servidor.
    /// El hilo receptor los encola aquí y el Update() los procesa en el hilo principal.
    /// </summary>
    private readonly Queue<string> incomingMessages = new();

    // ========== INICIALIZACIÓN ==========
    /// <summary>
    /// Se ejecuta al inicio.
    /// IMPORTANTE: Ya NO se conecta automáticamente.
    /// Espera a que el usuario haga clic en "Crear Partida" o "Unirse"
    /// También hace que este GameObject persista entre escenas (Felix → Ender'sGame).
    /// </summary>
    void Start()
    {
        // CRÍTICO: Permitir ejecución en segundo plano para multiplayer
        Application.runInBackground = true;
        
        // Persistir entre escenas (lobby → gameplay)
        DontDestroyOnLoad(gameObject);
        
        // ⚠️ VALIDACIÓN: Verificar que otherPlayerPrefab esté asignado
        if (otherPlayerPrefab == null)
        {
            Debug.LogWarning("⚠️ ADVERTENCIA: otherPlayerPrefab NO está asignado en NetworkManager");
            Debug.LogWarning("⚠️ Los jugadores remotos NO aparecerán en el juego");
            Debug.LogWarning("⚠️ SOLUCIÓN: Selecciona NetworkManager en la escena Felix y asigna el prefab Player2D en el Inspector");
        }
        else
        {
            Debug.Log($"✅ otherPlayerPrefab asignado: {otherPlayerPrefab.name}");
        }
        
        // Iniciar heartbeat automático
        InvokeRepeating(nameof(SendHeartbeat), 2f, 2f); // Enviar cada 2 segundos
        
        Debug.Log("🎮 NetworkManager listo. Esperando crear/unirse a partida...");
    }

    /// <summary>
    /// Conecta al servidor y crea una nueva partida.
    /// Llamado desde el botón "Crear Partida" en la escena Felix.
    /// </summary>
    /// <param name="maxPlayers">Número de jugadores (2-4)</param>
    public void CreateGame(int maxPlayers)
    {
        try
        {
            Debug.Log($"🎯 Creando partida para {maxPlayers} jugadores...");
            
            // Crear cliente UDP con puerto local (0 = puerto automático pero fijo para este socket)
            udpClient = new UdpClient(0);
            serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);

            Debug.Log($"Cliente UDP creado. Puerto local: {((IPEndPoint)udpClient.Client.LocalEndPoint).Port}");
            Debug.Log($"Servidor remoto: {serverIp}:{serverPort}");

            // Marcar que estamos en lobby
            isInLobby = true;
            isGameStarted = false;

            // Iniciar hilo receptor PRIMERO
            StartReceiver();
            
            // LUEGO enviar mensaje CREATE
            SendCreateGame(maxPlayers);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error al crear partida: " + ex);
        }
    }

    /// <summary>
    /// Conecta al servidor y se une a una partida existente.
    /// Llamado desde el botón "Unirse a Partida" en la escena Felix.
    /// </summary>
    public void JoinGame()
    {
        try
        {
            Debug.Log("🔗 Uniéndose a partida...");
            
            // Crear cliente UDP con puerto local (0 = puerto automático pero fijo para este socket)
            udpClient = new UdpClient(0);
            serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);

            Debug.Log($"Cliente UDP creado. Puerto local: {((IPEndPoint)udpClient.Client.LocalEndPoint).Port}");
            Debug.Log($"Servidor remoto: {serverIp}:{serverPort}");

            // Marcar que estamos en lobby
            isInLobby = true;
            isGameStarted = false;

            // Iniciar hilo receptor PRIMERO
            StartReceiver();
            
            // LUEGO enviar mensaje JOIN
            SendJoin();
        }
        catch (Exception ex)
        {
            Debug.LogError("Error al unirse a partida: " + ex);
        }
    }
    
    // ========== LIMPIEZA ==========
    /// <summary>
    /// Limpia recursos cuando se cierra el juego.
    /// </summary>
    void OnApplicationQuit()
    {
        recvThread?.Abort();  // Detener hilo receptor
        udpClient?.Close();   // Cerrar conexión UDP
    }

    // ========== RECEPCIÓN DE MENSAJES ==========
    /// <summary>
    /// Inicia un hilo en segundo plano para recibir mensajes del servidor.
    /// Los mensajes se encolan en 'incomingMessages' y se procesan en Update().
    /// </summary>
    void StartReceiver()
    {
        Debug.Log("🎧 Iniciando hilo receptor...");
        recvThread = new Thread(() =>
        {
            var remoteEP = new IPEndPoint(IPAddress.Any, 0);
            try
            {
                Debug.Log("✅ Hilo receptor activo, esperando mensajes...");
                while (true)
                {
                    // Recibir datos del servidor
                    var data = udpClient.Receive(ref remoteEP);
                    var msg = Encoding.UTF8.GetString(data);
                    
                    // Debug.Log($"📩 Mensaje recibido del servidor: {msg}");
                    
                    // Encolar mensaje para procesar en hilo principal
                    lock (incomingMessages)
                        incomingMessages.Enqueue(msg);
                }
            }
            catch (ThreadAbortException) { Debug.Log("⚠️ Hilo receptor detenido"); }
            catch (Exception e) { Debug.LogError($"❌ Error en receptor: {e}"); }
        });
        recvThread.IsBackground = true;
        recvThread.Start();
    }

    // ========== UPDATE LOOP ==========
    /// <summary>
    /// Se ejecuta cada frame.
    /// Procesa mensajes recibidos del servidor en el hilo principal de Unity.
    /// </summary>
    void Update()
    {
        // Procesar todos los mensajes recibidos en el hilo principal
        // (Unity API solo puede usarse desde el hilo principal)
        lock (incomingMessages)
        {
            while (incomingMessages.Count > 0)
            {
                HandleMessage(incomingMessages.Dequeue());
            }
        }
    }

    // ========== MANEJO DE MENSAJES DEL SERVIDOR ==========
    /// <summary>
    /// Procesa mensajes JSON recibidos del servidor.
    /// 
    /// Tipos de mensajes:
    /// 1. JOIN_ACK: El servidor confirma tu conexión y te asigna un playerId único
    /// 2. LOBBY_UPDATE: Lista actualizada de jugadores en el lobby
    /// 3. GAME_START: El servidor indica que el juego comienza
    /// 4. STATE: El servidor envía las posiciones de todos los jugadores (durante el juego)
    /// </summary>
    void HandleMessage(string json)
    {
        // Debug.Log($"🔍 Procesando mensaje en hilo principal: {json}");
        
        // ===== MENSAJE ERROR =====
        if (json.Contains("\"type\":\"ERROR\""))
        {
            ErrorMessage error = JsonUtility.FromJson<ErrorMessage>(json);
            Debug.LogError($"❌ ERROR del servidor: {error.message}");
            return; // No procesar más
        }
        // ===== MENSAJE JOIN_ACK =====
        else if (json.Contains("\"type\":\"JOIN_ACK\"") || json.Contains("\"JOIN_ACK\""))
        {
            JoinAckMessage ack = JsonUtility.FromJson<JoinAckMessage>(json);
            playerId = ack.playerId;
            Debug.Log($"✅ Asignado playerId del servidor: {playerId}");
        }
        // ===== MENSAJE LOBBY_UPDATE =====
        else if (json.Contains("\"LOBBY_UPDATE\"") || json.Contains("\"type\":\"LOBBY_UPDATE\""))
        {
            LobbyUpdateMessage lobbyUpdate = JsonUtility.FromJson<LobbyUpdateMessage>(json);
            lobbyPlayers = lobbyUpdate.players;
            currentMaxPlayers = lobbyUpdate.maxPlayers;
            
            Debug.Log($"👥 Lobby actualizado: {lobbyUpdate.currentPlayers}/{lobbyUpdate.maxPlayers} jugadores");
            
            // Notificar a la UI con la información completa
            OnLobbyUpdated?.Invoke(lobbyPlayers, lobbyUpdate.currentPlayers, lobbyUpdate.maxPlayers);
        }
        // ===== MENSAJE GAME_START =====
        else if (json.Contains("\"GAME_START\"") || json.Contains("\"type\":\"GAME_START\""))
        {
            GameStartMessage gameStart = JsonUtility.FromJson<GameStartMessage>(json);
            isInLobby = false;
            isGameStarted = true;
            lobbyPlayers = gameStart.players;
            
            Debug.Log($"🎮 ¡JUEGO INICIADO! {gameStart.players.Count} jugadores");
            
            // Notificar para cargar la escena de juego
            OnGameStart?.Invoke();
        }
        // ===== MENSAJE STATE (solo durante el juego) =====
        else if (json.Contains("\"type\":\"STATE\""))
        {
            if (isGameStarted)
            {
                StateMessage state = JsonUtility.FromJson<StateMessage>(json);
                ApplyState(state);
            }
        }
        // ===== MENSAJE BULLET_SPAWN =====
        else if (json.Contains("\"type\":\"BULLET_SPAWN\""))
        {
            if (isGameStarted)
            {
                BulletSpawnMessage msg = JsonUtility.FromJson<BulletSpawnMessage>(json);
                SpawnRemoteBullet(msg);
            }
        }
        // ===== MENSAJE SPAWN_ENEMY =====
        else if (json.Contains("\"type\":\"SPAWN_ENEMY\""))
        {
            if (isGameStarted)
            {
                SpawnEnemyMessage msg = JsonUtility.FromJson<SpawnEnemyMessage>(json);
                SpawnEnemy(msg);
            }
        }
        // ===== MENSAJE SERVER_RESET =====
        else if (json.Contains("\"type\":\"SERVER_RESET\""))
        {
            Debug.LogWarning("🔄 Servidor indica RESET - Desconectando...");
            HandleServerDisconnection();
        }
    }
    
    /// <summary>
    /// Instancia una bala remota sincronizada por el servidor.
    /// </summary>
    void SpawnRemoteBullet(BulletSpawnMessage msg)
    {
        // Si es MI bala, no crearla (ya la disparé localmente)
        if (msg.playerId == playerId)
        {
            // Debug.Log($"💥 Ignorando mi propia bala: {msg.bulletId}");
            return;
        }
        
        // Buscar el Player2D local para usar su pool de misiles
        var player2D = FindFirstObjectByType<Player2D>();
        if (player2D != null)
        {
            // Usar el pool de misiles existente
            player2D.FireRemoteMissile(msg.posX, msg.posY);
            // Debug.Log($"💥 Bala remota disparada desde pool: {msg.bulletId} del jugador {msg.playerId} en ({msg.posX:F2}, {msg.posY:F2})");
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró Player2D para disparar bala remota");
        }
    }
    
    /// <summary>
    /// Genera un enemigo sincronizado desde el servidor.
    /// </summary>
    void SpawnEnemy(SpawnEnemyMessage msg)
    {
        // Buscar el ControllerObjectsDamage para usar su pool de enemigos
        var controller = FindFirstObjectByType<ControllerObjectsDamage>();
        if (controller != null)
        {
            // Llamar al método público para spawnnear desde el servidor con el índice exacto
            controller.SpawnEnemyFromServer(msg.enemyIndex, msg.posX, msg.posY, msg.velocityX, msg.velocityY);
            Debug.Log($"👾 Enemigo sincronizado: {msg.enemyId} índice {msg.enemyIndex} en ({msg.posX:F2}, {msg.posY:F2})");
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró ControllerObjectsDamage para spawnnear enemigo");
        }
    }

    // ========== ENVÍO DE MENSAJES AL SERVIDOR ==========
    /// <summary>
    /// Envía mensaje CREATE al servidor para crear una nueva partida.
    /// </summary>
    void SendCreateGame(int maxPlayers)
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "UnityPlayer");
        
        var msg = new CreateGameMessage
        {
            type = "CREATE",
            name = playerName,

            maxPlayers = maxPlayers
        };
        SendJson(msg);
        
        Debug.Log($"📤 Enviando CREATE con nombre: {playerName}, maxPlayers: {maxPlayers}");
    }

    /// <summary>
    /// Envía mensaje JOIN al servidor para unirse a una partida existente.
    /// </summary>
    void SendJoin()
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "UnityPlayer");
        
        var msg = new JoinMessage
        {
            type = "JOIN",
            name = playerName
        };
        SendJson(msg);
        
        Debug.Log($"📤 Enviando JOIN con nombre: {playerName}");
    }

    /// <summary>
    /// Envía tu posición actual al servidor cada 50ms.
    /// El servidor recopila las posiciones de todos y las redistribuye en mensajes STATE.
    /// </summary>
    void SendPosition()
    {
        if (!players.ContainsKey(playerId)) return;

        var pos = players[playerId].transform;
        var msg = new PosMessage
        {
            type = "POS",
            playerId = playerId,
            posX = pos.position.x,
            posY = pos.position.y,
            rotation = pos.rotation.eulerAngles.z  // Rotación en Z para 2D
        };
        SendJson(msg);
    }

    /// <summary>
    /// Envía mensaje SHOOT al servidor cuando el jugador dispara.
    /// El servidor gestiona la creación de balas y las difunde a todos los clientes.
    /// </summary>
    public void SendShoot(float posX, float posY)
    {
        if (!isGameStarted) return;
        
        var msg = new ShootMessage
        {
            type = "SHOOT",
            playerId = playerId,
            posX = posX,
            posY = posY
        };
        SendJson(msg);
        Debug.Log($"🔫 SHOOT enviado: ({posX:F2}, {posY:F2})");
    }

    /// <summary>
    /// Utilidad para enviar cualquier objeto como JSON al servidor.
    /// </summary>
    void SendJson(object obj)
    {
        string json = JsonUtility.ToJson(obj);
        byte[] data = Encoding.UTF8.GetBytes(json);
        udpClient.Send(data, data.Length, serverEndPoint);
    }

    // ========== APLICAR ESTADO DEL SERVIDOR ==========
    /// <summary>
    /// Aplica el estado de todos los jugadores recibido del servidor.
    /// 
    /// IMPORTANTE: Ya NO crea shadow players.
    /// Solo crea y actualiza naves para OTROS jugadores reales.
    /// Tu nave (Player2D) ya existe en la escena y se controla localmente.
    /// </summary>
    void ApplyState(StateMessage state)
    {
        if (state.players == null) return;

        foreach (var p in state.players)
        {
            // Si es TU playerId, ignorar (no necesitas actualizar tu propia nave desde el servidor)
            if (p.playerId == playerId)
            {
                continue;  // Tu nave se mueve localmente con Player2D
            }

            // ===== CREAR NAVE PARA OTRO JUGADOR =====
            if (!players.ContainsKey(p.playerId))
            {
                // ⚠️ VALIDACIÓN CRÍTICA: Verificar que otherPlayerPrefab esté asignado
                if (otherPlayerPrefab == null)
                {
                    // Intentar usar localPlayer2D como template
                    if (localPlayer2D != null)
                    {
                        Debug.LogWarning($"⚠️ otherPlayerPrefab es null, usando localPlayer2D como template");
                        otherPlayerPrefab = localPlayer2D;
                    }
                    else
                    {
                        Debug.LogError($"❌ ERROR CRÍTICO: otherPlayerPrefab NO está asignado y localPlayer2D tampoco existe!");
                        Debug.LogError($"❌ No se puede crear jugador remoto: {p.name} ({p.playerId})");
                        Debug.LogError($"❌ SOLUCIÓN: Crea un prefab de Player2D o asigna otherPlayerPrefab en el Inspector");
                        return;
                    }
                }
                
                Vector3 spawnPos = new Vector3(p.posX, p.posY, 0f);
                
                var obj = Instantiate(otherPlayerPrefab, spawnPos, Quaternion.identity);
                obj.name = "RemotePlayer_" + p.playerId;
                
                // Desactivar el script Player2D en el jugador remoto (no debe controlarse localmente)
                var player2DScript = obj.GetComponent<Player2D>();
                if (player2DScript != null)
                {
                    player2DScript.enabled = false;
                    Debug.Log($"🔇 Desactivado script Player2D en jugador remoto");
                }
                
                // Hacer el Rigidbody2D kinematic para que no sea empujado por físicas pero siga detectando colisiones
                var rigidbody = obj.GetComponent<Rigidbody2D>();
                if (rigidbody != null)
                {
                    rigidbody.bodyType = RigidbodyType2D.Kinematic; // No recibe fuerzas pero detecta colisiones
                    Debug.Log($"🔧 Rigidbody2D configurado como Kinematic en jugador remoto");
                }
                
                players[p.playerId] = obj;
                
                Debug.Log($"✅ Creado jugador remoto: {p.name} ({p.playerId}) en posición {spawnPos}");
            }

            // ===== ACTUALIZAR POSICIÓN DEL JUGADOR REMOTO =====
            var go = players[p.playerId];
            if (go != null)
            {
                Vector3 targetPos = new Vector3(p.posX, p.posY, 0);
                Quaternion targetRot = Quaternion.Euler(0, 0, p.rotation);
                
                // Interpolar suavemente
                go.transform.position = Vector3.Lerp(go.transform.position, targetPos, 0.3f);
                go.transform.rotation = Quaternion.Slerp(go.transform.rotation, targetRot, 0.3f);
            }
        }
    }

    /// <summary>
    /// Inicia el envío de posición al servidor.
    /// Se llama cuando el juego comienza (después de GAME_START).
    /// Busca automáticamente Player2D si no está asignado en el Inspector.
    /// </summary>
    public void StartSendingPosition()
    {
        // Si localPlayer2D no está asignado, buscarlo en la escena
        if (localPlayer2D == null)
        {
            localPlayer2D = GameObject.FindGameObjectWithTag("Player");
            if (localPlayer2D == null)
            {
                localPlayer2D = GameObject.Find("Player2D");
            }
            
            if (localPlayer2D != null)
            {
                Debug.Log($"🔍 Encontrado Player2D automáticamente: {localPlayer2D.name}");
            }
            else
            {
                Debug.LogError("❌ No se encontró Player2D. Asegúrate de que existe en la escena Ender'sGame con tag 'Player'");
                return;
            }
        }
        
        // Registrar tu nave local en el diccionario
        if (!players.ContainsKey(playerId))
        {
            players[playerId] = localPlayer2D;
            Debug.Log($"✅ Player local registrado: {localPlayer2D.name}");
        }
        
        // Programar envío de posición cada 50ms (20 Hz)
        InvokeRepeating(nameof(SendPosition), 0.1f, 0.05f);
        Debug.Log("📡 Iniciando envío de posición al servidor...");
    }
    
    /// <summary>
    /// Envía un mensaje HEARTBEAT al servidor para indicar que seguimos conectados
    /// </summary>
    private void SendHeartbeat()
    {
        if (udpClient == null || string.IsNullOrEmpty(playerId) || playerId == "auto")
        {
            return; // No enviar si no estamos conectados
        }

        try
        {
            // Crear mensaje manualmente (JsonUtility no serializa objetos anónimos)
            string json = $"{{\"type\":\"HEARTBEAT\",\"playerId\":\"{playerId}\"}}";
            byte[] data = Encoding.UTF8.GetBytes(json);
            udpClient.Send(data, data.Length, serverEndPoint);
            
            // Log temporal para debugging
            Debug.Log($"💓 HEARTBEAT enviado - {playerId}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"⚠️ Error enviando HEARTBEAT: {e.Message}");
        }
    }
    
    /// <summary>
    /// Maneja la desconexión del servidor o reset
    /// </summary>
    private void HandleServerDisconnection()
    {
        Debug.LogWarning("⚠️ ========== SERVIDOR DESCONECTADO ==========");
        Debug.LogWarning("⚠️ El servidor ha reseteado o perdió la conexión");
        Debug.LogWarning("⚠️ Volviendo a la escena Felix...");
        
        // Limpiar estado local
        isGameStarted = false;
        isInLobby = false;
        
        // Detener heartbeat
        CancelInvoke(nameof(SendHeartbeat));
        
        // Cerrar conexión UDP
        if (udpClient != null)
        {
            udpClient.Close();
            udpClient = null;
        }
        
        // Volver al lobby
        UnityEngine.SceneManagement.SceneManager.LoadScene("Felix");
    }

}

// ========== CLASES DE DATOS (MENSAJES JSON) ==========
// Estas clases deben coincidir EXACTAMENTE con las del servidor Java

/// <summary>
/// Mensaje JOIN: Cliente → Servidor
/// Se envía al conectar para registrarse en el servidor
/// </summary>
/// <summary>
/// Mensaje JOIN: Cliente → Servidor
/// Un cliente solicita unirse a una partida existente
/// </summary>
[Serializable]
public class JoinMessage
{
    public string type;      // "JOIN"
    public string name;      // Nombre del jugador
}

/// <summary>
/// Mensaje JOIN_ACK: Servidor → Cliente
/// El servidor confirma la unión y asigna un ID único al jugador
/// </summary>
[Serializable]
public class JoinAckMessage
{
    public string type;      // "JOIN_ACK"
    public string playerId;  // ID único asignado por el servidor (ej: "player_123456")
}

/// <summary>
/// Mensaje CREATE: Cliente → Servidor
/// El host crea una nueva partida especificando el número de jugadores
/// </summary>
[Serializable]
public class CreateGameMessage
{
    public string type;         // "CREATE"
    public string name;         // Nombre del host
    public int maxPlayers;      // Número máximo de jugadores (2-4)
}

/// <summary>
/// Mensaje LOBBY_UPDATE: Servidor → Clientes
/// El servidor notifica cambios en el lobby (jugadores que entran/salen)
/// </summary>
[Serializable]
public class LobbyUpdateMessage
{
    public string type;                    // "LOBBY_UPDATE"
    public int currentPlayers;             // Jugadores actuales en lobby
    public int maxPlayers;                 // Jugadores necesarios
    public List<LobbyPlayer> players;      // Lista de jugadores en lobby
}

/// <summary>
/// Información de un jugador en el lobby
/// </summary>
[Serializable]
public class LobbyPlayer
{
    public string playerId;
    public string name;
    public bool isHost;     // true si es el que creó la partida
}

/// <summary>
/// Mensaje GAME_START: Servidor → Clientes
/// El servidor notifica que todos los jugadores están listos y el juego comienza
/// </summary>
[Serializable]
public class GameStartMessage
{
    public string type;                    // "GAME_START"
    public List<LobbyPlayer> players;      // Lista final de jugadores
}

/// <summary>
/// Mensaje POS: Cliente → Servidor
/// Se envía cada 50ms con la posición actual del jugador
/// </summary>
[Serializable]
public class PosMessage
{
    public string type;      // "POS"
    public string playerId;  // Tu ID asignado por el servidor
    public float posX;       // Posición X
    public float posY;       // Posición Y (juego 2D en plano XY)
    public float rotation;   // Rotación
}

/// <summary>
/// Estado de un jugador dentro del mensaje STATE
/// </summary>
[Serializable]
public class PlayerState
{
    public string type;
    public string playerId;
    public string name;
    public float posX;
    public float posY;
    public float rotation;
}

/// <summary>
/// Mensaje STATE: Servidor → Cliente
/// El servidor envía periódicamente las posiciones de TODOS los jugadores
/// </summary>
[Serializable]
public class StateMessage
{
    public string type;              // "STATE"
    public long time;                // Timestamp del servidor
    public List<PlayerState> players; // Lista de todos los jugadores conectados
}

/// <summary>
/// Mensaje ERROR: Servidor → Cliente
/// El servidor envía un mensaje de error cuando algo falla
/// </summary>
[Serializable]
public class ErrorMessage
{
    public string type;      // "ERROR"
    public string message;   // Mensaje de error
}
