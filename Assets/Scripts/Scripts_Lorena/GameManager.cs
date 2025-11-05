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
        if (scoreText) scoreText.text = score.ToString();
    }

    public void OnPlayerDeath()
    {
        var spawner = FindObjectOfType<Spawner>();
        if (spawner) spawner.enabled = false;

        if (gameOverPanel) gameOverPanel.SetActive(true);

        int best = PlayerPrefs.GetInt("best", 0);
        if (score > best) PlayerPrefs.SetInt("best", score);
        if (bestText) bestText.text = "Best: " + PlayerPrefs.GetInt("best", 0);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
