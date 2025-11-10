using System;
using UnityEngine;

// ========== MENSAJES PARA GAMEPLAY MULTIPLAYER ==========

/// <summary>
/// Mensaje SHOOT: Cliente → Servidor
/// El cliente solicita disparar una bala.
/// </summary>
[Serializable]
public class ShootMessage
{
    public string type;          // "SHOOT"
    public string playerId;      // ID del jugador que dispara
    public float posX;           // Posición X del disparo
    public float posY;           // Posición Y del disparo
    public long timestamp;       // Timestamp del cliente
}

/// <summary>
/// Mensaje SPAWN_ENEMY: Servidor → Clientes
/// El servidor notifica que se ha creado un nuevo enemigo.
/// </summary>
[Serializable]
public class SpawnEnemyMessage
{
    public string type;          // "SPAWN_ENEMY"
    public string enemyId;       // ID único del enemigo
    public string enemyType;     // "asteroid" o "missile"
    public int enemyIndex;       // Índice específico del enemigo (0=asteroid, 1-3=naves)
    public float posX;           // Posición X inicial
    public float posY;           // Posición Y inicial
    public float velocityX;      // Velocidad en X
    public float velocityY;      // Velocidad en Y
    public float rotation;       // Rotación inicial
}

/// <summary>
/// Mensaje BULLET_SPAWN: Servidor → Clientes
/// El servidor notifica que se ha creado una nueva bala.
/// </summary>
[Serializable]
public class BulletSpawnMessage
{
    public string type;          // "BULLET_SPAWN"
    public string bulletId;      // ID único de la bala
    public string playerId;      // ID del jugador que disparó
    public float posX;           // Posición X inicial
    public float posY;           // Posición Y inicial
    public float velocityX;      // Velocidad en X
    public float velocityY;      // Velocidad en Y
}

/// <summary>
/// Mensaje DESTROY_ENEMY: Servidor → Clientes
/// El servidor notifica que un enemigo debe ser destruido.
/// </summary>
[Serializable]
public class DestroyEnemyMessage
{
    public string type;          // "DESTROY_ENEMY"
    public string enemyId;       // ID del enemigo a destruir
}

/// <summary>
/// Mensaje DESTROY_BULLET: Servidor → Clientes
/// El servidor notifica que una bala debe ser destruida.
/// </summary>
[Serializable]
public class DestroyBulletMessage
{
    public string type;          // "DESTROY_BULLET"
    public string bulletId;      // ID de la bala a destruir
}

/// <summary>
/// Mensaje UPDATE_SCORE: Servidor → Clientes
/// El servidor notifica que la puntuación de un jugador ha cambiado.
/// </summary>
[Serializable]
public class UpdateScoreMessage
{
    public string type;          // "UPDATE_SCORE"
    public string playerId;      // ID del jugador
    public int points;           // Puntos ganados en esta acción
    public int score;            // Puntuación total del jugador
}
