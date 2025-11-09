using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int score = 0;
    public TMP_Text scoreText;
    public GameObject gameOverPanel;
    public TMP_Text bestText;
    public TMP_Text countText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateScoreUI();
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText) scoreText.text = "Score: " + score.ToString();

        if (countText) countText.text = "Score: " + score.ToString();
    }
    
   public void OnPlayerDeath()
    {
        // Detener el spawner
        var spawner = FindObjectOfType<Spawner>();
        if (spawner) spawner.enabled = false;

        // Activar panel de Game Over
        if (gameOverPanel) gameOverPanel.SetActive(true);

        // Actualizar récord
        int best = PlayerPrefs.GetInt("best", 0);
        if (score > best) PlayerPrefs.SetInt("best", score);
        if (bestText) bestText.text = "Best: " + PlayerPrefs.GetInt("best", 0);

        // Buscar todos los objetos Ring y cambiar su variable playerDead
        Ring[] allRings = FindObjectsOfType<Ring>();
        foreach (Ring ring in allRings)
        {
            ring.playerDead = true;
        }

        // Buscar todos los objetos Ring y cambiar su variable playerDead
        Obstacle[] allObstacles = FindObjectsOfType<Obstacle>();
        foreach (Obstacle obstacle in allObstacles)
        {
            obstacle.playerDead = true;
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
