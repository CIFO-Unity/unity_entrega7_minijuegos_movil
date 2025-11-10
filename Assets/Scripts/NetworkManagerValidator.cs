using UnityEngine;

/// <summary>
/// Script de validación para NetworkManager
/// Verifica que todas las referencias estén correctamente asignadas
/// </summary>
public class NetworkManagerValidator : MonoBehaviour
{
    void Start()
    {
        // Buscar NetworkManager en la escena
        NetworkManager nm = FindObjectOfType<NetworkManager>();
        
        if (nm == null)
        {
            Debug.LogError("❌ No se encontró NetworkManager en la escena");
            return;
        }
        
        Debug.Log("=== 🔍 VALIDACIÓN DE NETWORKMANAGER ===");
        
        // Verificar otherPlayerPrefab
        if (nm.otherPlayerPrefab == null)
        {
            Debug.LogError("❌ CRÍTICO: otherPlayerPrefab NO está asignado!");
            Debug.LogError("⚠️ Los jugadores remotos NO aparecerán");
            Debug.LogError("📝 SOLUCIÓN:");
            Debug.LogError("   1. Selecciona el GameObject NetworkManager en la jerarquía");
            Debug.LogError("   2. En el Inspector, busca el campo 'Other Player Prefab'");
            Debug.LogError("   3. Arrastra el prefab Player2D desde la carpeta Prefabs");
        }
        else
        {
            Debug.Log($"✅ otherPlayerPrefab asignado: {nm.otherPlayerPrefab.name}");
        }
        
        // Verificar configuración de red
        Debug.Log($"🌐 Server IP: {nm.serverIp}");
        Debug.Log($"🔌 Server Port: {nm.serverPort}");
        Debug.Log($"🏃 Run In Background: {Application.runInBackground}");
        
        Debug.Log("=========================================");
    }
}
