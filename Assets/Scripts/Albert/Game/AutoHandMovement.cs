using UnityEngine;

public class AutoHandMovement : MonoBehaviour
{
    #region Variables

    [Header("Movimiento")]
    [SerializeField]
    private float moveSpeed = 3f;              // Velocidad base del movimiento horizontal
    [SerializeField]
    private float upwardSpeed = 1f;            // Velocidad del movimiento ascendente al tocar la pantalla
    [SerializeField]
    private float upwardDistance = 2f;         // Distancia que sube la mano al tocar la pantalla
    [SerializeField]
    private float horizontalRange = 1.5f;      // Rango de movimiento lateral desde el centro
    
    private int direction = 1;                 // Dirección del movimiento: 1 = derecha, -1 = izquierda
    private bool movingUp = false;             // Indica si la mano está subiendo actualmente
    private bool stopped = false;              // Indica si el movimiento está completamente detenido
    private float targetY;                     // Posición Y objetivo cuando la mano sube
    private bool hitDanger = false;            // Indica si colisionó con una zona peligrosa
    private bool triggeredEnd = false;         // Evita múltiples llamadas al finalizar la fase

    private Vector3 initialPosition;           // Posición inicial de la mano al comenzar
    private GameManagerAlbert gameManager;           // Referencia al gestor principal del juego

    private float currentMoveSpeed;            // Velocidad actual que puede ser modificada externamente

    private Collider2D[] fingerColliders;      // Array de colliders del dedo para detectar colisiones

    #endregion

    #region Start & Update

    // Inicializar posiciones, velocidad y colliders al comenzar
    private void Start()
    {
        // Guardar la posición inicial de la mano
        initialPosition = transform.position;
        // Establecer la posición Y objetivo igual a la actual
        targetY = transform.position.y;
        // Inicializar la velocidad actual con la velocidad base
        currentMoveSpeed = moveSpeed;

        // Obtener todos los colliders del dedo y sus hijos
        fingerColliders = GetComponentsInChildren<Collider2D>();

        // Desactivar todos los colliders al inicio (se activarán al tocar)
        foreach (var col in fingerColliders)
            col.enabled = false;
    }

    // Actualizar el movimiento y detectar input del jugador cada frame
    private void Update()
    {
        // Si el movimiento está detenido, no hacer nada
        if (stopped)
            return;

        // Detectar toque en pantalla para iniciar movimiento ascendente
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && !movingUp)
        {
            // Activar el estado de movimiento ascendente
            movingUp = true;
            // Calcular la posición objetivo sumando la distancia de subida
            targetY = initialPosition.y + upwardDistance;

            // Reproducir sonido de subida del dedo
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySound("SonidoSubirDedo");

            // Activar colliders para detectar colisiones durante el movimiento ascendente
            foreach (var col in fingerColliders)
                col.enabled = true;
        }

        // Si la mano está subiendo
        if (movingUp)
        {
            // Si tocó zona peligrosa, detener y finalizar fase como fallo
            if (hitDanger)
            {
                movingUp = false;
                stopped = true;
                StartSceneRestart(false);
                return;
            }

            // Mover hacia arriba hasta alcanzar la posición objetivo
            Vector3 pos = transform.position;
            // Usar MoveTowards para movimiento suave hacia el objetivo
            pos.y = Mathf.MoveTowards(pos.y, targetY, upwardSpeed * Time.deltaTime);
            transform.position = pos;

            // Verificar si llegó a la posición objetivo (con pequeño margen de error)
            if (Mathf.Abs(pos.y - targetY) < 0.01f)
            {
                // Desactivar movimiento ascendente
                movingUp = false;
                // Detener completamente el movimiento
                stopped = true;
                // Notificar que la fase terminó exitosamente
                StartSceneRestart(true);
            }
        }
        // Si no está subiendo, ejecutar movimiento lateral
        else
        {
            MoveSideToSide();
        }
    }

    #endregion

    #region Movimiento

    // Mover la mano de lado a lado con rebotes en los límites
    private void MoveSideToSide()
    {
        // Obtener posición actual
        Vector3 pos = transform.position;
        // Calcular nuevo desplazamiento horizontal según dirección y velocidad
        pos.x += direction * currentMoveSpeed * Time.deltaTime;

        // Calcular límites izquierdo y derecho desde la posición inicial
        float leftLimit = initialPosition.x - horizontalRange;
        float rightLimit = initialPosition.x + horizontalRange;

        // Verificar si alcanzó el límite izquierdo
        if (pos.x <= leftLimit)
        {
            // Ajustar posición exacta al límite
            pos.x = leftLimit;
            // Cambiar dirección hacia la derecha
            direction = 1;

            // Reproducir sonido de cambio de dirección
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySound("SonidoCambiarDireccionMano");
        }
        // Verificar si alcanzó el límite derecho
        else if (pos.x >= rightLimit)
        {
            // Ajustar posición exacta al límite
            pos.x = rightLimit;
            // Cambiar dirección hacia la izquierda
            direction = -1;

            // Reproducir sonido de cambio de dirección
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySound("SonidoCambiarDireccionMano");
        }

        // Aplicar la nueva posición al transform
        transform.position = pos;
    }

    // Detener completamente el movimiento de la mano
    public void StopMovement()
    {
        // Marcar como detenido
        stopped = true;
        // Desactivar movimiento ascendente
        movingUp = false;
    }

    // Establecer una nueva velocidad de movimiento
    public void SetMoveSpeed(float newSpeed)
    {
        // Actualizar la velocidad actual con el nuevo valor
        currentMoveSpeed = newSpeed;
    }

    #endregion

    #region Detección de Colisiones

    // Detectar colisión con zonas seguras o peligrosas
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Si ya se procesó el final de la fase, ignorar
        if (triggeredEnd) return;

        // Verificar colisión con zona peligrosa
        if (other.CompareTag("DangerZone"))
        {
            // Marcar que tocó zona peligrosa
            hitDanger = true;

            // Reproducir sonido de error aleatorio
            if (SoundManager.Instance != null)
            {
                // Array con los tres sonidos de error disponibles
                string[] errores = { "SonidoError1", "SonidoError2", "SonidoError3" };
                // Elegir uno al azar
                string sonidoElegido = errores[Random.Range(0, errores.Length)];
                // Reproducir el sonido seleccionado
                SoundManager.Instance.PlaySound(sonidoElegido);
            }

            // Notificar que la fase terminó con fallo
            StartSceneRestart(false);
        }
        // Verificar colisión con zona segura
        else if (other.CompareTag("SafeZone"))
        {
            // Reproducir sonido de acierto
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySound("SonidoAcierto");

            // Notificar que la fase terminó con éxito
            StartSceneRestart(true);
        }
    }

    #endregion

    #region Control de Fase

    // Notificar al GameManager el resultado de la fase
    private void StartSceneRestart(bool success)
    {
        // Si ya se procesó el final, ignorar para evitar duplicados
        if (triggeredEnd) return;
        // Marcar como procesado
        triggeredEnd = true;

        // Notificar al GameManager el resultado (éxito o fallo)
        if (gameManager != null)
            gameManager.OnPhaseEnd(success);
    }

    // Restablecer la mano a su estado inicial para la siguiente fase
    public void ResetHand()
    {
        // Restaurar posición inicial
        transform.position = initialPosition;
        // Desactivar movimiento ascendente
        movingUp = false;
        // Permitir movimiento nuevamente
        stopped = false;
        // Resetear indicador de zona peligrosa
        hitDanger = false;
        // Permitir nuevo procesamiento de final de fase
        triggeredEnd = false;
        // Restablecer dirección hacia la derecha
        direction = 1;

        // Desactivar colliders hasta el próximo toque
        foreach (var col in fingerColliders)
            col.enabled = false;
    }

    // Asignar referencia al GameManager
    public void SetGameManager(GameManagerAlbert gm)
    {
        // Guardar la referencia del GameManager
        gameManager = gm;
    }

    #endregion
}