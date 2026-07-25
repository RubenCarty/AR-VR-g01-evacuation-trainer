using UnityEngine;

/// <summary>
/// Zona segura fuera del edificio (patio). Cuando la cabeza del usuario
/// entra en su radio durante la evacuación, finaliza el simulacro y
/// dispara el reporte de evaluación.
/// </summary>
public class SalidaSegura : MonoBehaviour
{
    [Tooltip("Radio horizontal de la zona segura en metros")]
    [SerializeField] private float radio = 2.5f;

    private Transform cabeza;

    private void Start()
    {
        if (Camera.main != null)
            cabeza = Camera.main.transform;
    }

    private void Update()
    {
        if (cabeza == null || GestorEvacuacion.Instance == null)
            return;

        if (GestorEvacuacion.Instance.Estado != GestorEvacuacion.EstadoSimulacro.Evacuacion)
            return;

        Vector3 delta = cabeza.position - transform.position;
        delta.y = 0f;

        if (delta.sqrMagnitude <= radio * radio)
            GestorEvacuacion.Instance.FinalizarSimulacro();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radio);
    }
}
