using System.Runtime.Serialization;
using UnityEngine;

public class Player2D : MonoBehaviour
{
    [Header("Joystick Floating")]
    [SerializeField]
    private FloatingJoystick fj;
    [Header("Speed Player")]
    [SerializeField]
    [Range(1.0f, 10.0f)]
    private float speed = 1.5f;

    [Header("UI")]
    [SerializeField]
    private GameObject accumulator;
    [SerializeField]
    private GameObject explosions;

    [SerializeField]
    private GameObject missilesPlayer;
    private int indexArrayMissiles;

    private int indexExplosionsArray;
    private bool selectorTrigger = false;//tenemos 3 triggers en player2d y da multiples contactos con un solo impacto

    [SerializeField]
    private GameObject[] listSounds;
    
    // Referencia al NetworkManager para modo multiplayer
    private NetworkManager networkManager;
    private bool isMultiplayer = false;
    
    private bool gameStarted = false; // Control para saber si el juego ha comenzado
    private float fireTimer = 0f;
    private float fireInterval = 1.0f; // Intervalo de disparo en segundos
    
    void Start()
    {
        indexExplosionsArray = 0;
        indexArrayMissiles = 0;
        
        // Detectar si estamos en modo multiplayer
        networkManager = FindFirstObjectByType<NetworkManager>();
        if (networkManager != null && networkManager.isGameStarted)
        {
            isMultiplayer = true;
            Debug.Log("🌐 Player2D en modo MULTIPLAYER - Disparos se enviarán al servidor");
        }
        else
        {
            isMultiplayer = false;
            Debug.Log("🎮 Player2D en modo SINGLEPLAYER - Disparos locales");
        }
        
        // Iniciar disparos automáticamente
        gameStarted = true;
        fireTimer = 0f;
        Debug.Log("🚀 Player2D: Disparos activados");
    }
    
    // Método público para iniciar el juego (llamado por GameModeSelector)
    public void StartGame()
    {
        if (gameStarted) return; // Evitar múltiples inicios
        
        gameStarted = true;
        fireTimer = 0f; // Reiniciar timer para disparo inmediato
        Debug.Log("🚀 Player2D: Juego iniciado - Disparos activados");
    }
    
    // Método público para detener el juego (útil para pausas o volver al menú)
    public void StopGame()
    {
        gameStarted = false;
        fireTimer = 0f;
        Debug.Log("⏸️ Player2D: Juego detenido - Disparos desactivados");
    }

    // Update is called once per frame
    void Update()
    {
        // Movimiento del jugador
        this.gameObject.transform.Translate(fj.Horizontal * Time.deltaTime * speed, fj.Vertical * Time.deltaTime * speed, 0.0f);
        
        // Sistema de disparo manual (no depende de InvokeRepeating)
        if (gameStarted)
        {
            fireTimer += Time.deltaTime;
            
            if (fireTimer >= fireInterval)
            {
                if (isMultiplayer)
                {
                    // En multiplayer, enviar mensaje SHOOT al servidor
                    FireMultiplayer();
                }
                else
                {
                    // En singleplayer, disparar localmente
                    FireMissile();
                }
                fireTimer = 0f; // Reiniciar timer
            }
        }
    }
    
    /// <summary>
    /// Envía mensaje SHOOT al servidor en modo multiplayer.
    /// El servidor creará la bala y la sincronizará con todos los clientes.
    /// </summary>
    private void FireMultiplayer()
    {
        if (networkManager != null)
        {
            Vector3 pos = transform.position;
            networkManager.SendShoot(pos.x, pos.y);
            
            // IMPORTANTE: También disparar localmente para feedback inmediato
            // El servidor validará y sincronizará con otros jugadores
            FireMissile();
        }
    }
    
    /// <summary>
    /// Dispara un misil localmente.
    /// </summary>
    private void FireMissile()
    {
        listSounds[1].gameObject.GetComponent<AudioSource>().Play();
        if (indexArrayMissiles >= missilesPlayer.transform.childCount)
            indexArrayMissiles = 0;
        missilesPlayer.gameObject.transform.GetChild(indexArrayMissiles).transform.position = new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.y);
        missilesPlayer.gameObject.transform.GetChild(indexArrayMissiles).GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0.0f, 10.0f);
        indexArrayMissiles++;
    }
    
    /// <summary>
    /// Método público para que NetworkManager dispare misiles remotos usando el mismo pool
    /// </summary>
    public void FireRemoteMissile(float posX, float posY)
    {
        if (missilesPlayer == null) return;
        
        if (indexArrayMissiles >= missilesPlayer.transform.childCount)
            indexArrayMissiles = 0;
        
        // Reutilizar misil del pool
        Transform missile = missilesPlayer.transform.GetChild(indexArrayMissiles);
        missile.position = new Vector2(posX, posY);
        missile.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0.0f, 10.0f);
        
        indexArrayMissiles++;
        
        // Debug.Log($"🚀 Misil remoto disparado desde pool en ({posX:F2}, {posY:F2})");
    }

    private void HideExplosion()
    {
        for (int i = 0; i < explosions.gameObject.transform.childCount; i++)
        {
            explosions.gameObject.transform.GetChild(i).gameObject.SetActive(false);
        }

        selectorTrigger = false; // permitimos futuras colisiones
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignorar colisiones con misiles propios y naves fantasma
        if (other.gameObject.CompareTag("MissilePlayer")) return; // si colisiona con el propio misil no hacemos nada
        if (other.gameObject.CompareTag("Albert")) return; // ignorar naves fantasma
        
        listSounds[0].gameObject.GetComponent<AudioSource>().Play();
        if (selectorTrigger) return; // si ya ha colisionado, no hacemos nada ya que tiene 3 triggers 1 impacto directo 3 llamadas al OnTriggerEnter2D --> MAL
        selectorTrigger = true; // marcamos que ya ocurrió una colisión

        explosions.gameObject.transform.GetChild(indexExplosionsArray).transform.position = this.gameObject.transform.position;
        explosions.gameObject.transform.GetChild(indexExplosionsArray).gameObject.SetActive(true);

        Invoke("HideExplosion", 0.5f);

        indexExplosionsArray++;

        if (indexExplosionsArray >= explosions.transform.childCount)
            indexExplosionsArray = 0;

        //Destroy(other.gameObject);
        
        //lo descomentaremos para produccion
        Destroy(this.gameObject);
        accumulator.gameObject.GetComponent<UIController>().GameOver();
        
    }

}
