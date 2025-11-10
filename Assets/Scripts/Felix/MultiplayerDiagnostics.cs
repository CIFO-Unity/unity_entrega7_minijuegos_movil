using UnityEngine;

/// <summary>
/// Script de diagnóstico para verificar configuración de multiplayer
/// Muestra información crítica en consola al iniciar
/// </summary>
public class MultiplayerDiagnostics : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== 🔍 DIAGNÓSTICO DE MULTIPLAYER ===");
        Debug.Log($"✅ Run In Background: {Application.runInBackground}");
        Debug.Log($"🖥️ Plataforma: {Application.platform}");
        Debug.Log($"🪟 Resolución: {Screen.width}x{Screen.height}");
        Debug.Log($"📺 Fullscreen Mode: {Screen.fullScreenMode}");
        Debug.Log($"🎮 Target Frame Rate: {Application.targetFrameRate}");
        
        #if UNITY_EDITOR
        Debug.Log("🔧 Ejecutando en UNITY EDITOR");
        #else
        Debug.Log("📦 Ejecutando en BUILD");
        #endif
        
        Debug.Log("=====================================");
    }
}
