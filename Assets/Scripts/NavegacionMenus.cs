using System.Data;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NavegacionMenus : MonoBehaviour
{

    private string escenaCreditos = "Creditos";

    private string escenaMiniJuegos = "MiniJuegos";

    private string escenaAlbert = "Albert";
    private string escenaLorena = "Lorena";
    private string escenaFelix = "Felix";
    private string escenaMainMenu = "MainMenu";


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(escenaMainMenu);
        }
    }
    public void CargarEscenaAlbert()
    {
        SceneManager.LoadScene(escenaAlbert);
    }

    public void CargarEscenaLorena()
    {
        SceneManager.LoadScene(escenaLorena);
    }

    public void CargarEscenaFelix()
    {
        SceneManager.LoadScene(escenaFelix);
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


}
