using UnityEngine;
using UnityEngine.SceneManagement;

public class AndroidBackButton : MonoBehaviour
{
    [Header("Configuración de escena")]
    [Tooltip("Nombre de la escena a cargar al pulsar el botón 'Atrás' en Android.")]
    [SerializeField] private string backSceneName = "MainMenu";

    void Update()
    {
        // Solo funciona en Android
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                string currentScene = SceneManager.GetActiveScene().name;

                // Si ya estás en la escena destino, salir del juego
                if (currentScene == backSceneName)
                {
                    Application.Quit();
                }
                else
                {
                    // Cargar la escena indicada en el inspector
                    if (!string.IsNullOrEmpty(backSceneName))
                        SceneManager.LoadScene(backSceneName);
                    else
                        Debug.LogWarning("No se ha asignado ninguna escena en el campo 'backSceneName'.");
                }
            }
        }
    }
}
