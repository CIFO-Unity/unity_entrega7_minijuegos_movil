using UnityEngine;
using UnityEngine.SceneManagement;

public class NavegacionMenus : MonoBehaviour
{
    private string escenaMainMenu = "MainMenu";
    private string escenaCreditos = "Creditos";
    private string escenaMiniJuegos = "MiniJuegos";
    private string escenaAlbert = "Albert";
    private string escenaLorena = "Lorena";
    private string escenaFelix = "Felix";

    void Update()
    {
        // Funciona con ESC en PC y con botón Atrás en Android real
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Si no estamos ya en el menú principal
            if (SceneManager.GetActiveScene().name == escenaMiniJuegos || SceneManager.GetActiveScene().name == escenaCreditos)
            {
                SceneManager.LoadScene(escenaMainMenu);
            }else if (SceneManager.GetActiveScene().name == escenaAlbert || SceneManager.GetActiveScene().name == escenaLorena || SceneManager.GetActiveScene().name == escenaFelix)
            {
                SceneManager.LoadScene(escenaMiniJuegos);
            }
        }
    }

    public void CargarEscenaAlbert() => SceneManager.LoadScene(escenaAlbert);
    public void CargarEscenaLorena() => SceneManager.LoadScene(escenaLorena);
    public void CargarEscenaFelix() => SceneManager.LoadScene(escenaFelix);
    public void CargarEscenaCreditos() => SceneManager.LoadScene(escenaCreditos);
    public void CargarEscenaMiniJuegos() => SceneManager.LoadScene(escenaMiniJuegos);

    public void SalirDelJuego()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
