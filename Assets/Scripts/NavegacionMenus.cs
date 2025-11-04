using UnityEngine;

public class NavegacionMenus : MonoBehaviour
{
    private string escenaJugar = "Jugar";

    private string escenaCreditos = "Creditos";

    private string escenaMiniJuegos = "MiniJuegos";


    public void CargarEscenaJugar()
    {
        SceneManager.LoadScene(escenaJugar);
    }

    public void CargarEscenaCreditos()
    {
        SceneManager.LoadScene(escenaCreditos);
    }

    public void CargarEscenaMiniJuegos()
    {
        SceneManager.LoadScene(escenaMiniJuegos);
    }

    public void SalirDelJuego()
    {

#if UNITY_EDITOR
            // En el editor, detiene el modo Play
            UnityEditor.EditorApplication.isPlaying = false;
#else
        // En build, cierra la aplicación
        Application.Quit();
#endif
    }

    public void VolverAlMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
