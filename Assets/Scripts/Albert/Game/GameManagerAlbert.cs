using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GameManagerAlbert : MonoBehaviour
{
    #region Variables

    [Header("Referencias")]
    [SerializeField]
    private AutoHandMovement[] hands;                     // Array de manos
    [SerializeField]
    private GameObject[] noses;                           // Array de narices

    [Header("Vidas")]
    [SerializeField]
    private int initialLives = 4;                         // Cantidad de vidas iniciales del jugador
    [SerializeField]
    private GameObject vidasContainer;                    // Contenedor donde se muestran los corazones
    [SerializeField]
    private Sprite blinkHeartSprite;                      // Sprite que se usa cuando un corazón parpadea

    [Header("Barra de tiempo")]
    [SerializeField]
    private Slider timeSlider;                            // Slider que muestra el tiempo restante
    [SerializeField]
    private UnityEngine.UI.Image imageSliderBackground;   // Fondo visual de la barra de tiempo

    [Header("Dificultad Progresiva - Velocidad")]
    [SerializeField]
    private float initialHandSpeed = 3f;                  // Velocidad inicial de la mano
    [SerializeField]
    private float speedIncreasePerPhase = 0.4f;           // Velocidad que se aumenta en cada fase
    [SerializeField]
    private float maxHandSpeed = 8f;                      // Velocidad máxima que la mano puede alcanzar

    [Header("Dificultad Progresiva - Tiempo")]
    [SerializeField]
    private float faseDuration = 10f;                     // Duración base de una fase
    [SerializeField]
    private float timeDecreasePerPhase = 0.5f;            // Tiempo que se reduce en cada fase
    [SerializeField]
    private float minPhaseDuration = 3f;                  // Tiempo mínimo permitido en una fase

    [Header("UI")]
    [SerializeField]
    private RectTransform leftPanel;                      // Panel izquierdo que se mueve en las transiciones
    [SerializeField]
    private RectTransform rightPanel;                     // Panel derecho que se mueve en las transiciones
    [SerializeField]
    private TMP_Text phaseMessage;                        // Mensaje que muestra el texto de la fase
    [SerializeField]
    private TMP_Text gameOverMessage;                     // Mensaje que aparece al perder todas las vidas
    [SerializeField]
    private TMP_Text bestRecordText;                      // Texto que muestra el récord obtenido
    [SerializeField]
    private Button retryButton;                           // Botón para reiniciar la partida
    [SerializeField]
    private Button backButton;                            // Botón para volver al menú principal

    [Header("UI - Configuración")]
    [SerializeField]
    private float panelMoveDuration = 0.5f;               // Duración de la animación de movimiento de paneles
    [SerializeField]
    private float messageDuration = 2f;                   // Tiempo que el mensaje de fase permanece visible
    [SerializeField]
    private float delayAfterSuccess = 1.0f;               // Tiempo de espera tras acertar una fase
    [SerializeField]
    private float delayBeforeHeartBlink = 0.3f;           // Tiempo antes de que el corazón empiece a parpadear
    [SerializeField]
    private float heartBlinkDuration = 1.0f;              // Duración del parpadeo de un corazón
    [SerializeField]
    private float delayAfterHeartBlink = 1.0f;            // Tiempo tras el parpadeo antes de continuar

    [Header("Datos")]
    [SerializeField]
    private RecordDataBase recordDatabase;                // Referencia a la base de datos del récord

    private float timeRemaining;                          // Tiempo restante de la fase actual
    private bool faseActiva = false;                      // Indica si la fase está activa
    private bool tiempoPausado = false;                   // Indica si el temporizador está pausado
    private float currentPhaseDuration;                   // Duración actual de la fase (ajustada por dificultad)
    private float currentHandSpeed;                       // Velocidad actual de la mano (ajustada por dificultad)

    private int currentLives;                             // Vidas actuales del jugador
    private int currentPhase = 0;                         // Índice de la mano/nariz actual
    private int totalPhasesPlayed = 0;                    // Número total de fases completadas
    private bool isGameOver = false;                      // Indica si el juego ha terminado

    #endregion

    #region Start & Update

    // Inicializar valores, ocultar UI y comenzar la primera fase
    private void Start()
    {
        // Establecer valores iniciales de dificultad
        currentPhaseDuration = faseDuration;
        currentHandSpeed = initialHandSpeed;

        // Ocultar elementos de UI que no deben verse al inicio
        if (timeSlider != null) timeSlider.gameObject.SetActive(false);
        if (vidasContainer != null) vidasContainer.SetActive(false);
        if (imageSliderBackground != null) imageSliderBackground.gameObject.SetActive(false);
        
        // Desactivar todas las manos y narices
        foreach (var h in hands) h.gameObject.SetActive(false);
        foreach (var n in noses) n.gameObject.SetActive(false);

        // Ocultar elementos de la pantalla de Game Over
        if (bestRecordText != null) bestRecordText.gameObject.SetActive(false);
        if (retryButton != null) retryButton.gameObject.SetActive(false);
        if (backButton != null) backButton.gameObject.SetActive(false);

        // Iniciar la secuencia de la primera fase
        StartCoroutine(ShowStartPhase());
    }

    // Actualizar temporizador y detectar cuando se acaba el tiempo
    private void Update()
    {
        // Modificar el tamaño de los panels laterales en caso de que cambie la resolución del dispositivo
        // Implementado para poder cambiar de móvil en el simulador en tiempo de ejecución y que los paneles adquieran el tamaño que deben tener con el nuevo dispositivo; sí, me sobra tiempo
        RectTransform canvasRect = GetParentCanvas().GetComponent<RectTransform>();
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;
        float halfWidth = canvasWidth / 2f;

        // Panel izquierdo
        leftPanel.sizeDelta = new Vector2(halfWidth, 0f);
        // Panel derecho
        rightPanel.sizeDelta = new Vector2(halfWidth, 0f);

        // Solo actualizar el tiempo si la fase está activa, no hay game over y no está pausado
        if (faseActiva && !isGameOver && !tiempoPausado)
        {
            // Restar tiempo cada frame
            timeRemaining -= Time.deltaTime;

            // Actualizar el valor visual del slider
            if (timeSlider != null)
                timeSlider.value = Mathf.Max(0, timeRemaining);

            // Verificar si se acabó el tiempo
            if (timeRemaining <= 0f)
            {
                // Desactivar la fase
                faseActiva = false;

                // Reproducir sonido de tiempo agotado
                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlaySound("SonidoTimeOut");
                
                // Detener el movimiento de la mano
                if (hands[currentPhase] != null)
                    hands[currentPhase].StopMovement();
                
                // Finalizar la fase como fallida
                OnPhaseEnd(false);
            }
        }

        // Pausar el temporizador al tocar la pantalla
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            tiempoPausado = true;
    }

    #endregion

    #region Control de Fases

    // Mostrar la primera fase del juego
    private IEnumerator ShowStartPhase()
    {
        // Esperar un frame para asegurar que todo está inicializado
        yield return null;
        yield return new WaitForSeconds(0.1f);

        // Configurar los paneles para que ocupen la mitad de la pantalla cada uno
        SetupPanelsAsHalves();

        // Establecer las vidas iniciales
        currentLives = initialLives;

        // Asegurar que todas las manos y narices están desactivadas
        foreach (var h in hands) h.gameObject.SetActive(false);
        foreach (var n in noses) n.gameObject.SetActive(false);

        // Cerrar los paneles
        yield return StartCoroutine(MovePanels(open: false));

        // Mostrar mensaje de "Stage 1"
        phaseMessage.text = "Stage 1";
        phaseMessage.color = Color.white;
        phaseMessage.gameObject.SetActive(true);

        // Reproducir sonido de cambio de fase
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("SonidoCambioFase1");

        // Preparar la fase sin activar elementos visualmente
        ActivatePhase(0, false);

        // Esperar mientras el mensaje está visible
        yield return new WaitForSeconds(messageDuration);

        // Activar todos los elementos de la fase
        ActivatePhase(0, true);

        // Ocultar el mensaje
        phaseMessage.gameObject.SetActive(false);

        // Abrir los paneles
        yield return StartCoroutine(MovePanels(open: true));

        // Iniciar el temporizador
        StartTimer();
    }

    // Activar elementos de la fase y configurar dificultad
    private void ActivatePhase(int index, bool activateElements = true)
    {
        // No activar nada si el juego ha terminado
        if (isGameOver) return;

        // Desactivar todas las manos y narices primero
        foreach (var h in hands) if (h != null) h.gameObject.SetActive(false);
        foreach (var n in noses) if (n != null) n.gameObject.SetActive(false);

        // Calcular el índice de la fase usando módulo para ciclar entre las manos disponibles
        int phaseIndex = index % hands.Length;
        currentPhase = phaseIndex;

        // Si se deben activar los elementos visualmente
        if (activateElements)
        {
            // Incrementar contador de fases jugadas
            totalPhasesPlayed++;

            // Actualizar la dificultad según las fases completadas
            UpdateDifficulty();

            // Activar la mano correspondiente y establecer su velocidad
            if (hands[currentPhase] != null) 
            {
                hands[currentPhase].gameObject.SetActive(true);
                hands[currentPhase].SetMoveSpeed(currentHandSpeed);
            }

            // Activar la nariz correspondiente
            if (noses != null && noses.Length > currentPhase && noses[currentPhase] != null)
                noses[currentPhase].gameObject.SetActive(true);

            // Mostrar el slider de tiempo y el contenedor de vidas
            if (imageSliderBackground != null) imageSliderBackground.gameObject.SetActive(true);
            if (timeSlider != null) timeSlider.gameObject.SetActive(true);
            if (vidasContainer != null) vidasContainer.SetActive(true);

            // Configurar el slider con la duración actual de la fase
            if (timeSlider != null)
            {
                timeSlider.maxValue = currentPhaseDuration;
                timeSlider.value = currentPhaseDuration;
            }
        }

        // Resetear la mano a su posición inicial
        hands[currentPhase].ResetHand();
        // Asignar la referencia del GameManager a la mano
        hands[currentPhase].SetGameManager(this);

        // Si se activaron los elementos, despausar el temporizador
        if (activateElements)
        {
            tiempoPausado = false;

            // Configurar nuevamente el slider por seguridad
            if (timeSlider != null)
            {
                timeSlider.maxValue = currentPhaseDuration;
                timeSlider.value = currentPhaseDuration;
            }
        }
    }

    // Manejar el final de una fase (éxito o fallo)
    public void OnPhaseEnd(bool success)
    {
        // No procesar si el juego ya terminó
        if (isGameOver) return;

        // Detener el temporizador
        faseActiva = false;

        // Si el jugador tuvo éxito
        if (success)
        {
            // Preparar mensaje de la siguiente fase
            string message = $"Stage {totalPhasesPlayed + 1}";
            // Iniciar transición de éxito
            StartCoroutine(ShowTransitionAfterSuccess(message, currentPhase + 1));
        }
        // Si el jugador falló
        else
        {
            // Verificar si es la última vida
            if (currentLives <= 1)
            {
                // Marcar como game over
                isGameOver = true;

                // Guardar el récord
                if (recordDatabase != null)
                    recordDatabase.GuardarRecord(totalPhasesPlayed);

                // Iniciar secuencia de Game Over
                StartCoroutine(GameOverRoutineWithDelay());
            }
            // Si aún quedan vidas
            else
            {
                // Preparar mensaje de la siguiente fase
                string message = $"Stage {totalPhasesPlayed + 1}";
                // Iniciar transición de fallo
                StartCoroutine(ShowTransitionAfterFailure(message, currentPhase + 1));
            }
        }
    }

    // Mostrar transición tras acertar una fase
    private IEnumerator ShowTransitionAfterSuccess(string message, int nextPhase)
    {
        // Esperar un tiempo antes de mostrar la transición
        yield return new WaitForSeconds(delayAfterSuccess);
        // Ejecutar la transición con el mensaje
        yield return StartCoroutine(ShowTransition(message, Color.white, nextPhase));
    }

    // Mostrar transición tras fallar una fase (con parpadeo de corazón)
    private IEnumerator ShowTransitionAfterFailure(string message, int nextPhase)
    {
        // Esperar antes de iniciar el parpadeo del corazón
        yield return new WaitForSeconds(delayBeforeHeartBlink);
        
        // Hacer parpadear el corazón que se va a perder
        yield return StartCoroutine(BlinkHeart());

        // Restar una vida
        currentLives--;
        // Actualizar la UI de vidas
        UpdateLivesUI();

        // Esperar después del parpadeo
        yield return new WaitForSeconds(delayAfterHeartBlink);
        
        // Ejecutar la transición con el mensaje
        yield return StartCoroutine(ShowTransition(message, Color.white, nextPhase));
    }

    // Mostrar transición entre fases con mensaje y paneles
    private IEnumerator ShowTransition(string message, Color color, int nextPhase)
    {
        // Cerrar los paneles
        yield return StartCoroutine(MovePanels(open: false));

        // Resetear visualmente la barra de tiempo al máximo
        if (timeSlider != null)
            timeSlider.value = currentPhaseDuration;

        // Configurar y mostrar el mensaje de fase
        phaseMessage.text = message;
        phaseMessage.color = color;
        phaseMessage.gameObject.SetActive(true);

        // Reproducir sonido de cambio de fase
        if (SoundManager.Instance != null)
        {
            // Array con los sonidos disponibles
            string[] sonidosPorFase =
            {
                "SonidoCambioFase1",
                "SonidoCambioFase2",
                "SonidoCambioFase3",
                "SonidoCambioFase4"
            };

            // Calcular índice usando módulo para ciclar entre los sonidos
            int indice = (totalPhasesPlayed % sonidosPorFase.Length);

            // Reproducir el sonido correspondiente
            SoundManager.Instance.PlaySound(sonidosPorFase[indice]);
        }

        // Preparar la siguiente fase sin activar elementos
        if (!isGameOver && nextPhase >= 0)
            ActivatePhase(nextPhase, false);

        // Esperar mientras el mensaje está visible
        yield return new WaitForSeconds(messageDuration);

        // Activar todos los elementos de la siguiente fase
        ActivatePhase(nextPhase, true);

        // Ocultar el mensaje
        phaseMessage.gameObject.SetActive(false);

        // Si el juego no ha terminado, continuar
        if (!isGameOver && nextPhase >= 0)
        {
            // Abrir los paneles
            yield return StartCoroutine(MovePanels(open: true));
            // Iniciar el temporizador de la nueva fase
            StartTimer();
        }
    }

    #endregion

    #region Timer

    // Iniciar el temporizador de la fase actual
    private void StartTimer()
    {
        // Establecer el tiempo restante según la duración actual
        timeRemaining = currentPhaseDuration;
        // Activar la fase
        faseActiva = true;
        // Despausar el temporizador
        tiempoPausado = false;

        // Configurar el slider visual
        if (timeSlider != null)
        {
            // Establecer el valor máximo del slider
            timeSlider.maxValue = currentPhaseDuration;
            // Establecer el valor actual al máximo
            timeSlider.value = currentPhaseDuration;
        }
    }

    #endregion

    #region Dificultad Progresiva

    // Calcular y ajustar la dificultad según las fases completadas
    private void UpdateDifficulty()
    {
        // Calcular nueva velocidad sumando el incremento por fase
        // Limitar al máximo establecido usando Mathf.Min
        currentHandSpeed = Mathf.Min(
            initialHandSpeed + (totalPhasesPlayed * speedIncreasePerPhase),
            maxHandSpeed
        );

        // Calcular nueva duración restando la reducción por fase
        // Limitar al mínimo establecido usando Mathf.Max
        currentPhaseDuration = Mathf.Max(
            faseDuration - (totalPhasesPlayed * timeDecreasePerPhase),
            minPhaseDuration
        );
    }

    #endregion

    #region Vidas

    // Hacer parpadear el corazón que se va a perder
    private IEnumerator BlinkHeart()
    {
        // Verificar que existe el contenedor de vidas
        if (vidasContainer == null) yield break;

        // Buscar el corazón que corresponde a la vida que se va a perder
        Transform heartToLose = null;
        for (int i = vidasContainer.transform.childCount - 1; i >= 0; i--)
        {
            Transform heart = vidasContainer.transform.GetChild(i);
            // Encontrar el último corazón activo
            if (heart.gameObject.activeSelf && i == currentLives - 1)
            {
                heartToLose = heart;
                break;
            }
        }

        // Si no se encontró el corazón, salir
        if (heartToLose == null) yield break;

        // Obtener el componente Image del corazón
        Image heartImage = heartToLose.GetComponent<Image>();
        if (heartImage == null) yield break;

        // Guardar el sprite original del corazón
        Sprite originalSprite = heartImage.sprite;

        // Variables para controlar el parpadeo
        float elapsed = 0f;
        float interval = 0.12f;
        bool useBlinkSprite = true;

        // Bucle de parpadeo durante la duración establecida
        while (elapsed < heartBlinkDuration)
        {
            // Alternar entre sprite de parpadeo y sprite original
            heartImage.sprite = useBlinkSprite && blinkHeartSprite != null
                ? blinkHeartSprite
                : originalSprite;

            // Invertir el estado para el siguiente ciclo
            useBlinkSprite = !useBlinkSprite;

            // Reproducir sonido de parpadeo
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySound("SonidoBlinkCorazon");

            // Esperar el intervalo entre parpadeos
            yield return new WaitForSeconds(interval);
            // Acumular tiempo transcurrido
            elapsed += interval;
        }

        // Restaurar el sprite original al finalizar
        heartImage.sprite = originalSprite;
    }

    // Actualizar la UI de vidas según las vidas restantes
    private void UpdateLivesUI()
    {
        // Verificar que existe el contenedor de vidas
        if (vidasContainer == null) return;

        // Recorrer todos los corazones hijos
        for (int i = 0; i < vidasContainer.transform.childCount; i++)
        {
            Transform vida = vidasContainer.transform.GetChild(i);
            // Activar el corazón solo si su índice es menor que las vidas actuales
            vida.gameObject.SetActive(i < currentLives);
        }
    }

    #endregion

    #region Game Over

    // Ejecutar secuencia de Game Over con delay y parpadeo
    private IEnumerator GameOverRoutineWithDelay()
    {
        // Esperar antes de iniciar el parpadeo
        yield return new WaitForSeconds(delayBeforeHeartBlink);
        
        // Hacer parpadear el último corazón
        yield return StartCoroutine(BlinkHeart());

        // Restar la última vida
        currentLives--;
        // Actualizar la UI de vidas
        UpdateLivesUI();

        // Esperar después del parpadeo
        yield return new WaitForSeconds(delayAfterHeartBlink);
        
        // Mostrar la pantalla de Game Over
        yield return StartCoroutine(GameOverRoutine());
    }

    // Mostrar pantalla de Game Over con récord y botones
    private IEnumerator GameOverRoutine()
    {
        // Cerrar los paneles
        yield return StartCoroutine(MovePanels(open: false));

        // Ocultar el mensaje de fase
        if (phaseMessage != null)
            phaseMessage.gameObject.SetActive(false);

        // Mostrar el mensaje de Game Over
        if (gameOverMessage != null)
            gameOverMessage.gameObject.SetActive(true);

        // Mostrar el récord obtenido
        if (bestRecordText != null && recordDatabase != null)
        {
            // Obtener el récord guardado
            int bestRecord = recordDatabase.RetornarRecord();

            // Si el récord actual es mayor, usar el récord actual
            if (bestRecord < totalPhasesPlayed)
                bestRecord = totalPhasesPlayed;

            // Mostrar el score actual y el mejor récord
            bestRecordText.text = $"Score: {totalPhasesPlayed}\nBest Record: {bestRecord}";
            bestRecordText.gameObject.SetActive(true);
        }

        // Configurar el botón de reintentar
        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(true);
            // Limpiar listeners previos
            retryButton.onClick.RemoveAllListeners();
            // Añadir listener para reiniciar el juego
            retryButton.onClick.AddListener(() => RestartGame());
        }

        // Configurar el botón de volver
        if (backButton != null)
        {
            backButton.gameObject.SetActive(true);
            // Limpiar listeners previos
            backButton.onClick.RemoveAllListeners();
            // Añadir listener para volver al menú
            backButton.onClick.AddListener(() => GoBack());
        }
    }

    #endregion

    #region Paneles

    // Obtener el Canvas padre de los paneles
    private Canvas GetParentCanvas()
    {
        // Comenzar desde el padre del panel izquierdo
        Transform t = leftPanel.parent;
        // Subir en la jerarquía hasta encontrar un Canvas
        while (t != null)
        {
            var c = t.GetComponent<Canvas>();
            if (c != null) return c;
            t = t.parent;
        }
        return null;
    }

    // Configurar paneles para que cada uno ocupe la mitad exacta del Canvas
    private void SetupPanelsAsHalves()
    {
        // Verificar que ambos paneles existen
        if (leftPanel == null || rightPanel == null) return;

        // Obtener el RectTransform del Canvas padre
        RectTransform canvasRect = GetParentCanvas().GetComponent<RectTransform>();

        // Calcular dimensiones del Canvas
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        // Calcular la mitad del ancho del Canvas
        float halfWidth = canvasWidth / 2f;

        // Configurar panel izquierdo
        leftPanel.anchorMin = new Vector2(0f, 0f);          // Anclar esquina inferior izquierda
        leftPanel.anchorMax = new Vector2(0f, 1f);          // Anclar hasta esquina superior izquierda
        leftPanel.pivot = new Vector2(0f, 0.5f);            // Pivote en el borde izquierdo central
        leftPanel.sizeDelta = new Vector2(halfWidth, 0f);   // Establecer ancho a la mitad del Canvas
        leftPanel.anchoredPosition = new Vector2(0f, 0f);   // Posicionar en el borde izquierdo

        // Configurar panel derecho
        rightPanel.anchorMin = new Vector2(1f, 0f);         // Anclar esquina inferior derecha
        rightPanel.anchorMax = new Vector2(1f, 1f);         // Anclar hasta esquina superior derecha
        rightPanel.pivot = new Vector2(1f, 0.5f);           // Pivote en el borde derecho central
        rightPanel.sizeDelta = new Vector2(halfWidth, 0f);  // Establecer ancho a la mitad del Canvas
        rightPanel.anchoredPosition = new Vector2(0f, 0f);  // Posicionar en el borde derecho
    }

    // Animar paneles para abrirlos o cerrarlos
    private IEnumerator MovePanels(bool open)
    {
        // Obtener el Canvas padre
        Canvas parentCanvas = GetParentCanvas();
        // Obtener su RectTransform
        RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();

        // Variables de control de tiempo
        float elapsed = 0f;
        // Asegurar duración mínima de 0.01 segundos
        float duration = Mathf.Max(0.01f, panelMoveDuration);

        // Calcular dimensiones para la animación
        float canvasWidth = canvasRect.rect.width;
        // Distancia extra para mover los paneles fuera de la pantalla
        float outDistance = canvasWidth + 200f;

        // Guardar posiciones iniciales de ambos paneles
        Vector2 leftStart = leftPanel.anchoredPosition;
        Vector2 rightStart = rightPanel.anchoredPosition;

        // Calcular posiciones objetivo según si se abren o cierran
        // Si open=true: mover fuera de pantalla, si open=false: mover al centro
        Vector2 leftTarget = open
            ? new Vector2(-outDistance, leftStart.y)        // Mover hacia la izquierda fuera de pantalla
            : Vector2.zero;                                 // Mover al centro (cerrar)

        Vector2 rightTarget = open
            ? new Vector2(outDistance, rightStart.y)        // Mover hacia la derecha fuera de pantalla
            : Vector2.zero;                                 // Mover al centro (cerrar)

        // Bucle de animación
        while (elapsed < duration)
        {
            // Incrementar tiempo transcurrido
            elapsed += Time.deltaTime;
            // Calcular valor interpolado suavizado entre 0 y 1
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            // Interpolar posiciones de ambos paneles
            leftPanel.anchoredPosition = Vector2.Lerp(leftStart, leftTarget, t);
            rightPanel.anchoredPosition = Vector2.Lerp(rightStart, rightTarget, t);

            // Esperar al siguiente frame
            yield return null;
        }

        // Asegurar que los paneles lleguen exactamente a la posición final
        leftPanel.anchoredPosition = leftTarget;
        rightPanel.anchoredPosition = rightTarget;
    }
    
    #endregion

    #region Navegación

    // Reiniciar la escena actual
    private void RestartGame()
    {
        // Cargar la escena actual usando su índice en el Build Settings
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    // Volver al menú principal
    private void GoBack()
    {
        // Cargar la escena del menú principal por nombre
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuAlbert");
    }

    #endregion
}