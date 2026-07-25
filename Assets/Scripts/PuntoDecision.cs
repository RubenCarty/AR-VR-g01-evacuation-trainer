using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Punto de control invisible colocado en lugares clave de la ruta
/// (escalera, ascensor, pasillos). Cuando la cabeza del usuario entra en su
/// radio durante la evacuación, registra la decisión (correcta o incorrecta)
/// en el EvaluadorDecisiones. Solo se registra una vez por simulacro.
/// </summary>
public class PuntoDecision : MonoBehaviour
{
    private static readonly List<PuntoDecision> todos = new List<PuntoDecision>();

    [Header("Configuración de la decisión")]
    [Tooltip("Descripción que aparece en subtítulos y en el reporte final")]
    [SerializeField] private string descripcion = "tomar la escalera de emergencia";
    [Tooltip("¿Pasar por aquí es una buena decisión de evacuación?")]
    [SerializeField] private bool esCorrecta = true;
    [Tooltip("Radio horizontal de detección en metros")]
    [SerializeField] private float radio = 1.5f;

    private bool visitado;
    private Transform cabeza;

    private void OnEnable() => todos.Add(this);
    private void OnDisable() => todos.Remove(this);

    private void Start()
    {
        if (Camera.main != null)
            cabeza = Camera.main.transform;
    }

    private void Update()
    {
        if (visitado || cabeza == null || GestorEvacuacion.Instance == null)
            return;

        // Las decisiones solo cuentan mientras se evacúa.
        if (GestorEvacuacion.Instance.Estado != GestorEvacuacion.EstadoSimulacro.Evacuacion &&
            GestorEvacuacion.Instance.Estado != GestorEvacuacion.EstadoSimulacro.Alarma)
            return;

        Vector3 delta = cabeza.position - transform.position;
        delta.y = 0f;

        if (delta.sqrMagnitude <= radio * radio)
        {
            visitado = true;
            if (EvaluadorDecisiones.Instance != null)
                EvaluadorDecisiones.Instance.RegistrarDecision(descripcion, esCorrecta);
        }
    }

    /// <summary>Configura el punto desde el constructor de escena.</summary>
    public void Configurar(string nuevaDescripcion, bool correcta, float nuevoRadio)
    {
        descripcion = nuevaDescripcion;
        esCorrecta = correcta;
        radio = nuevoRadio;
    }

    public static void ReiniciarTodos()
    {
        foreach (var p in todos) p.visitado = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = esCorrecta ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radio);
    }
}
