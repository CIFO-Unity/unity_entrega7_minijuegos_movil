
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Para TextMeshPro InputField

public class GameModeSelector : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private TMP_InputField playerNameInput; // Arrastra aquí el InputField del nombre
    
    // Claves para guardar datos
    private const string GAME_MODE_KEY = "GameMode";
    private const string PLAYER_NAME_KEY = "PlayerName";

    void Start()
    {
        // Cargar el nombre guardado en PlayerPrefs si existe
        if (playerNameInput != null)
        {
            if (PlayerPrefs.HasKey(PLAYER_NAME_KEY))
            {
                string savedName = PlayerPrefs.GetString(PLAYER_NAME_KEY);
                playerNameInput.text = savedName;
                Debug.Log($"📝 Nombre cargado desde PlayerPrefs: {savedName}");
            }
            
            // Suscribirse al evento de cambio de texto para guardar automáticamente
            playerNameInput.onEndEdit.AddListener(OnPlayerNameChanged);
        }
    }

    // Método que se ejecuta cuando el usuario termina de editar el nombre
    private void OnPlayerNameChanged(string newName)
    {
        if (!string.IsNullOrEmpty(newName.Trim()))
        {
            PlayerPrefs.SetString(PLAYER_NAME_KEY, newName.Trim());
            PlayerPrefs.Save();
            Debug.Log($"💾 Nombre guardado automáticamente: {newName.Trim()}");
        }
    }

    void OnDestroy()
    {
        // Desuscribirse del evento para evitar memory leaks
        if (playerNameInput != null)
        {
            playerNameInput.onEndEdit.RemoveListener(OnPlayerNameChanged);
        }
    }

    // Método para el botón "Single Player"
    public void SelectSinglePlayer()
    {
        Debug.Log("🎯 Single Player seleccionado");
        
        // Guardar elección en PlayerPrefs
        PlayerPrefs.SetString(GAME_MODE_KEY, "SinglePlayer");
        PlayerPrefs.Save();
        
        SceneManager.LoadScene("Ender'sGame");
    }

    // Método para el botón "Multiplayer"
    public void SelectMultiplayer()
    {
        Debug.Log("🌐 Multiplayer seleccionado");
        
        // Obtener el nombre del jugador del InputField
        string playerName = playerNameInput != null ? playerNameInput.text.Trim() : "";
        
        // Si está vacío, asignar un nombre por defecto
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "Player_" + Random.Range(1000, 9999);
            Debug.Log($"⚠️ Nombre vacío, usando nombre por defecto: {playerName}");
        }
        
        // Guardar elección y nombre en PlayerPrefs
        PlayerPrefs.SetString(GAME_MODE_KEY, "Multiplayer");
        PlayerPrefs.SetString(PLAYER_NAME_KEY, playerName);
        PlayerPrefs.Save();
        
        Debug.Log($"✅ Nombre de jugador guardado: {playerName}");
        
        SceneManager.LoadScene("Ender'sGame");
    }
}

