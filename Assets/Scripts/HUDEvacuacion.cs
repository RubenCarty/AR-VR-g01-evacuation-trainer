using TMPro;
using UnityEngine;

/// <summary>
/// HUD en espacio de mundo que sigue suavemente a la cámara VR.
/// Muestra el estado del simulacro, el cronómetro de evacuación y,
/// al finalizar, el reporte completo del EvaluadorDecisiones.
/// </summary>
public class HUDEvacuacion : MonoBehaviour
{
    [Header("Textos (asignados por el constructor de escena)")]
    [SerializeField] private TextMeshProUGUI textoEstado;
    [SerializeField] private TextMeshProUGUI textoCronometro;
    [SerializeField] private TextMeshProUGUI textoReporte;

    [Header("Seguimiento de cámara")]
    [Tooltip("Distancia frente a la cámara en metros")]
    [SerializeField] private float distancia = 2.2f;
    [Tooltip("Suavizado del seguimiento (mayor = más rígido)")]
    [SerializeField] private float suavizado = 4f;

    private Transform camara;

    private void Start()
    {
        if (Camera.main != null)
            camara = Camera.main.transform;

        if (GestorEvacuacion.Instance != null)
            GestorEvacuacion.Instance.OnCambioEstado += ActualizarEstado;

        if (textoReporte != null)
            textoReporte.text = "";
    }

    private void OnDestroy()
    {
        if (GestorEvacuacion.Instance != null)
            GestorEvacuacion.Instance.OnCambioEstado -= ActualizarEstado;
    }

    private void LateUpdate()
    {
        SeguirCamara();
        ActualizarCronometro();
    }

    /// <summary>El panel flota delante del usuario, siempre a la altura de los ojos.</summary>
    private void SeguirCamara()
    {
        if (camara == null)
        {
            if (Camera.main != null) camara = Camera.main.transform;
            return;
        }

        // Solo se usa la dirección horizontal de la mirada para evitar
        // que el HUD se incline al mirar al suelo o al techo.
        Vector3 adelante = camara.forward;
        adelante.y = 0f;
        if (adelante.sqrMagnitude < 0.001f)
            adelante = camara.up;
        adelante.Normalize();

        Vector3 destino = camara.position + adelante * distancia;
        transform.position = Vector3.Lerp(transform.position, destino, Time.deltaTime * suavizado);

        Vector3 mirada = transform.position - camara.position;
        mirada.y = 0f;
        if (mirada.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(mirada), Time.deltaTime * suavizado);
    }

    private void ActualizarCronometro()
    {
        if (textoCronometro == null || GestorEvacuacion.Instance == null)
            return;

        var gestor = GestorEvacuacion.Instance;
        if (gestor.Estado == GestorEvacuacion.EstadoSimulacro.Preparacion)
        {
            textoCronometro.text = "";
            return;
        }

        int min = Mathf.FloorToInt(gestor.TiempoEvacuacion / 60f);
        int seg = Mathf.FloorToInt(gestor.TiempoEvacuacion % 60f);
        textoCronometro.text = $"Tiempo {min:00}:{seg:00}";
    }

    private void ActualizarEstado(GestorEvacuacion.EstadoSimulacro estado)
    {
        if (textoEstado != null)
        {
            textoEstado.text = estado switch
            {
                GestorEvacuacion.EstadoSimulacro.Preparacion => "Exploración libre",
                GestorEvacuacion.EstadoSimulacro.Alarma => "¡ALARMA DE INCENDIO!",
                GestorEvacuacion.EstadoSimulacro.Evacuacion => "Evacúa a la zona segura",
                GestorEvacuacion.EstadoSimulacro.Finalizado => "Simulacro finalizado",
                _ => ""
            };
        }

        if (textoReporte != null)
        {
            textoReporte.text = estado == GestorEvacuacion.EstadoSimulacro.Finalizado
                && EvaluadorDecisiones.Instance != null
                ? EvaluadorDecisiones.Instance.UltimoReporte
                : "";
        }
    }

    /// <summary>Permite al constructor de escena conectar los textos del HUD.</summary>
    public void ConectarUI(TextMeshProUGUI estado, TextMeshProUGUI cronometro, TextMeshProUGUI reporte)
    {
        textoEstado = estado;
        textoCronometro = cronometro;
        textoReporte = reporte;
    }
}
