using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    // Método para el botón "Play"
    public void OnPlayButton()
    {
        // Recarga la escena actual (reinicia el juego)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Método para el botón "Back"
    public void OnBackButton()
    {
        // Carga la escena llamada "MiniJuegos"
        SceneManager.LoadScene("MiniJuegos");
    }
}
