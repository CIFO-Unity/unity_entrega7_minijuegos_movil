using UnityEngine;

/// <summary>
/// Carga dinámicamente el sprite de la nave según el nombre del jugador guardado en PlayerPrefs.
/// Coloca los sprites en: Assets/Sprites/Ships/[NombreJugador].png
/// </summary>
public class PlayerShipSpriteLoader : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Ruta de la carpeta donde están los sprites (desde Resources). Ej: 'Ships' para Resources/Ships")]
    [SerializeField] private string spriteFolderPath = "Ships";
    
    [Tooltip("Sprite por defecto si no se encuentra el sprite del jugador")]
    [SerializeField] private Sprite defaultSprite;
    
    [Tooltip("Es una nave fantasma (busca sprite con sufijo _Ghost)")]
    [SerializeField] private bool isGhostShip = false;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer == null)
        {
            Debug.LogError("❌ PlayerShipSpriteLoader: No se encontró SpriteRenderer en este GameObject");
            return;
        }

        LoadPlayerSprite();
    }

    /// <summary>
    /// Carga el sprite del jugador desde Resources
    /// </summary>
    void LoadPlayerSprite()
    {
        // Obtener el nombre del jugador desde PlayerPrefs
        string playerName = PlayerPrefs.GetString("PlayerName", "UnityPlayer");
        
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("⚠️ PlayerShipSpriteLoader: No hay nombre de jugador guardado");
            SetDefaultSprite();
            return;
        }

        // Construir la ruta del sprite
        string spritePath;
        if (isGhostShip)
        {
            spritePath = $"{spriteFolderPath}/{playerName}_Ghost";
        }
        else
        {
            spritePath = $"{spriteFolderPath}/{playerName}";
        }

        // Cargar el sprite desde Resources
        Sprite loadedSprite = Resources.Load<Sprite>(spritePath);

        if (loadedSprite != null)
        {
            spriteRenderer.sprite = loadedSprite;
            Debug.Log($"✅ Sprite cargado: {spritePath}");
        }
        else
        {
            Debug.LogWarning($"⚠️ No se encontró sprite en: Resources/{spritePath}.png - Usando sprite por defecto");
            SetDefaultSprite();
        }
    }

    /// <summary>
    /// Establece el sprite por defecto
    /// </summary>
    void SetDefaultSprite()
    {
        if (defaultSprite != null)
        {
            spriteRenderer.sprite = defaultSprite;
        }
    }

    /// <summary>
    /// Método público para cambiar el sprite manualmente
    /// </summary>
    public void ChangeSprite(string playerName)
    {
        if (spriteRenderer == null) return;

        string spritePath = isGhostShip 
            ? $"{spriteFolderPath}/{playerName}_Ghost" 
            : $"{spriteFolderPath}/{playerName}";

        Sprite loadedSprite = Resources.Load<Sprite>(spritePath);

        if (loadedSprite != null)
        {
            spriteRenderer.sprite = loadedSprite;
        }
        else
        {
            SetDefaultSprite();
        }
    }
}
