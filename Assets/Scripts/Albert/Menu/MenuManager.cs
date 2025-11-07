using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private RecordDataBase recordDatabase; // Referencia al sistema de récords
    [SerializeField] private TMP_Text bestRecordText;       // Texto donde se mostrará el récord

    private void Start()
    {
        if (recordDatabase == null)
        {
            Debug.LogWarning("No se ha asignado el RecordDataBase en el MenuManager.");
            return;
        }

        if (bestRecordText == null)
        {
            Debug.LogWarning("No se ha asignado el TMP_Text para mostrar el récord.");
            return;
        }

        int recordActual = recordDatabase.RetornarRecord();
        bestRecordText.text = $"Best record: {recordActual}";
    }

    // Cargar la escena del juego
    public void PlayGame()
    {
        SceneManager.LoadScene("MinijuegoAlbert");
    }

    // Volver al menú anterior
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MiniJuegos");
    }
}
