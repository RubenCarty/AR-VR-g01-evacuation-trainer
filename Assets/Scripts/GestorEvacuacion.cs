using System;
using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Cerebro del simulacro de evacuación. Máquina de estados que controla el flujo:
/// Preparacion → Alarma → Evacuacion → Finalizado.
/// Activa la alarma y las zonas de peligro, cronometra la evacuación y
/// notifica a los demás sistemas mediante eventos.
/// </summary>
public class GestorEvacuacion : MonoBehaviour
{
    public enum EstadoSimulacro
    {
        Preparacion,   // El usuario explora el aula antes de la alarma
        Alarma,        // Suena la alarma, se activan los peligros
        Evacuacion,    // El usuario debe llegar a la zona segura
        Finalizado     // Llegó a la salida: se muestra el reporte
    }

    public static GestorEvacuacion Instance { get; private set; }

    [Header("Tiempos del simulacro")]
    [Tooltip("Segundos de exploración libre antes de que suene la alarma")]
    [SerializeField] private float segundosPreparacion = 12f;
    [Tooltip("Tiempo objetivo de evacuación en segundos (referencia para la nota)")]
    [SerializeField] private float tiempoObjetivo = 120f;

    [Header("Referencias (auto-asignadas si se dejan vacías)")]
    [SerializeField] private AlarmaIncendio alarma;

    public EstadoSimulacro Estado { get; private set; } = EstadoSimulacro.Preparacion;
    public float TiempoEvacuacion { get; private set; }
    public float TiempoObjetivo => tiempoObjetivo;

    /// <summary>Se dispara cada vez que el simulacro cambia de estado.</summary>
    public event Action<EstadoSimulacro> OnCambioEstado;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (alarma == null)
            alarma = FindFirstObjectByType<AlarmaIncendio>();

        IniciarSimulacro();
    }

    private void Update()
    {
        // El cronómetro corre desde que suena la alarma hasta llegar a la salida.
        if (Estado == EstadoSimulacro.Alarma || Estado == EstadoSimulacro.Evacuacion)
            TiempoEvacuacion += Time.deltaTime;

#if ENABLE_INPUT_SYSTEM
        // Tecla R: reiniciar el simulacro (útil en demo y pruebas de escritorio).
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            ReiniciarSimulacro();
#endif
    }

    /// <summary>Arranca el flujo completo del simulacro desde cero.</summary>
    public void IniciarSimulacro()
    {
        StopAllCoroutines();
        TiempoEvacuacion = 0f;
        StartCoroutine(FlujoSimulacro());
    }

    private IEnumerator FlujoSimulacro()
    {
        CambiarEstado(EstadoSimulacro.Preparacion);
        if (AccesibilidadManager.Instance != null)
            AccesibilidadManager.Instance.MostrarSubtitulo(
                "Bienvenido al simulacro. Explora el aula con teletransporte (gatillo del control).",
                6f, esAlerta: false);

        yield return new WaitForSeconds(segundosPreparacion);

        ActivarAlarma();

        // Breve fase de alarma para que el usuario reaccione antes de evaluar decisiones.
        yield return new WaitForSeconds(3f);
        CambiarEstado(EstadoSimulacro.Evacuacion);
    }

    /// <summary>Enciende la alarma de incendio y activa todas las zonas de peligro.</summary>
    public void ActivarAlarma()
    {
        TiempoEvacuacion = 0f;
        CambiarEstado(EstadoSimulacro.Alarma);

        if (alarma != null)
            alarma.Activar();

        ZonaPeligro.ActivarTodas();

        if (AccesibilidadManager.Instance != null)
            AccesibilidadManager.Instance.MostrarSubtitulo(
                "¡ALARMA DE INCENDIO! Evacúa por la escalera. NO uses el ascensor.",
                8f, esAlerta: true);
    }

    /// <summary>Llamado por SalidaSegura cuando el usuario llega a la zona segura.</summary>
    public void FinalizarSimulacro()
    {
        if (Estado == EstadoSimulacro.Finalizado)
            return;

        CambiarEstado(EstadoSimulacro.Finalizado);

        if (alarma != null)
            alarma.Detener();

        if (EvaluadorDecisiones.Instance != null)
            EvaluadorDecisiones.Instance.GenerarReporte(TiempoEvacuacion, tiempoObjetivo);

        if (AccesibilidadManager.Instance != null)
            AccesibilidadManager.Instance.MostrarSubtitulo(
                "¡Llegaste a la zona segura! Revisa tu evaluación. Pulsa R para reiniciar.",
                10f, esAlerta: false);
    }

    /// <summary>Reinicia todo el simulacro: posición del jugador, evaluación y peligros.</summary>
    [ContextMenu("Reiniciar simulacro")]
    public void ReiniciarSimulacro()
    {
        if (alarma != null)
            alarma.Detener();

        ZonaPeligro.DesactivarTodas();
        PuntoDecision.ReiniciarTodos();

        if (EvaluadorDecisiones.Instance != null)
            EvaluadorDecisiones.Instance.Reiniciar();

        if (TeleportSystem.Instance != null)
            TeleportSystem.Instance.VolverAlInicio();

        IniciarSimulacro();
    }

    private void CambiarEstado(EstadoSimulacro nuevo)
    {
        Estado = nuevo;
        OnCambioEstado?.Invoke(nuevo);
        Debug.Log($"[GestorEvacuacion] Estado → {nuevo}");
    }
}
