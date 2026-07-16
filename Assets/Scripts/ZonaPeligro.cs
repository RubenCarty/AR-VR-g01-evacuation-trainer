using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zona de fuego o humo. Permanece dormida hasta que suena la alarma
/// (partículas y luz apagadas). Detecta al usuario por proximidad de la
/// cabeza (cámara VR), lo que funciona con teleportación sin depender de
/// física de Rigidbody, y penaliza a través del EvaluadorDecisiones.
/// </summary>
public class ZonaPeligro : MonoBehaviour
{
    private static readonly List<ZonaPeligro> todas = new List<ZonaPeligro>();

    [Header("Configuración")]
    [Tooltip("Nombre que se muestra en subtítulos y reporte (ej. 'el fuego del pasillo este')")]
    [SerializeField] private string nombreZona = "una zona de fuego";
    [Tooltip("Radio horizontal de peligro en metros")]
    [SerializeField] private float radio = 1.6f;
    [Tooltip("Segundos entre penalizaciones si el usuario permanece dentro")]
    [SerializeField] private float enfriamiento = 4f;

    private bool activa;
    private float proximaPenalizacion;
    private Transform cabeza;

    private void OnEnable() => todas.Add(this);
    private void OnDisable() => todas.Remove(this);

    private void Start()
    {
        if (Camera.main != null)
            cabeza = Camera.main.transform;
        SetActiva(false);
    }

    private void Update()
    {
        if (!activa || cabeza == null)
            return;

        // Distancia solo en el plano XZ: la altura de la cabeza no importa.
        Vector3 delta = cabeza.position - transform.position;
        delta.y = 0f;

        if (delta.sqrMagnitude <= radio * radio && Time.time >= proximaPenalizacion)
        {
            proximaPenalizacion = Time.time + enfriamiento;
            if (EvaluadorDecisiones.Instance != null)
                EvaluadorDecisiones.Instance.RegistrarContactoPeligro(nombreZona);
        }
    }

    /// <summary>Enciende o apaga el efecto visual (partículas y luces hijas).</summary>
    public void SetActiva(bool valor)
    {
        activa = valor;
        proximaPenalizacion = 0f;

        foreach (var ps in GetComponentsInChildren<ParticleSystem>(true))
        {
            if (valor) ps.Play();
            else ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        foreach (var luz in GetComponentsInChildren<Light>(true))
            luz.enabled = valor;
    }

    public static void ActivarTodas()
    {
        foreach (var z in todas) z.SetActiva(true);
    }

    public static void DesactivarTodas()
    {
        foreach (var z in todas) z.SetActiva(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radio);
    }
}
