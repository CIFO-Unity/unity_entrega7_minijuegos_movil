using UnityEditor;
using UnityEngine;

/// <summary>
/// Script del Editor que FUERZA "Run In Background" a estar siempre habilitado
/// Esto es crítico para testing de multiplayer local
/// </summary>
[InitializeOnLoad]
public class ForceRunInBackground
{
    static ForceRunInBackground()
    {
        // Se ejecuta cuando Unity carga
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            // Forzar runInBackground cuando se entra en Play Mode
            if (!Application.runInBackground)
            {
                Application.runInBackground = true;
                Debug.LogWarning("⚠️ Run In Background estaba deshabilitado. Se ha HABILITADO automáticamente para multiplayer.");
            }
            else
            {
                Debug.Log("✅ Run In Background ya está habilitado en el Editor");
            }
        }
    }
}
