using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    private int scoreUI;
    private int recordUI;

    [SerializeField]
    private GameObject scoreGameUI;

    [SerializeField]
    private GameObject scoreRecordUI;

    [SerializeField]
    private GameObject panelUI;


    public void increaseScore(int increment)
    {
        scoreUI += increment;
        updateScoreUI(scoreUI);
        if(scoreUI > recordUI)
        {
            recordUI = scoreUI;
            updateRecordUI(recordUI);
            PlayerPrefs.SetInt("RecordScore", recordUI);
        }
    }
    public void updateScoreUI(int newScore)
    {
        scoreGameUI.GetComponent<TMP_Text>().text = "Score: " + newScore.ToString();
    }

    public void updateRecordUI(int newRecord)
    {
        scoreRecordUI.GetComponent<TMP_Text>().text = "Record: " + newRecord.ToString();
    } 


    void Start()
    {
        scoreUI = 0;
        recordUI = 0;
        updateScoreUI(scoreUI);
        updateRecordUI(recordUI);

        //PlayerPrefs.DeleteKey("RecordScore");
        //PlayerPrefs.DeleteAll();
        

        if (PlayerPrefs.HasKey("RecordScore"))
        {
            recordUI = PlayerPrefs.GetInt("RecordScore");
            updateRecordUI(recordUI);
        }
        else
        {
            PlayerPrefs.SetInt("RecordScore", 0);
            recordUI = PlayerPrefs.GetInt("RecordScore");
            updateRecordUI(recordUI);
        }
    }

    public void GameOver()
    {
        panelUI.gameObject.SetActive(true);
        Invoke("timePauseExplosionPlayer2D", 1.0f);//tiempo espera para ver la explosion del player
    }

    public void timePauseExplosionPlayer2D()
    {
        Time.timeScale = 0.0f;//congelamos todo el juego
    }

    public void newGame()
    {
        Time.timeScale = 1.0f;//reanudamos el juego
        SceneManager.LoadScene("Felix");
    }
}
