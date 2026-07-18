using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zona de fuego o humo con PROPAGACIÓN: permanece dormida hasta que suena
/// la alarma y, una vez activa, crece durante `duracionPropagacion` segundos
/// (más emisión de partículas, más luz y mayor radio de peligro), presionando
/// al usuario a evacuar rápido. Detecta al usuario por proximidad de la
/// cabeza (cámara VR), lo que funciona con teleportación sin depender de
/// física de Rigidbody, y penaliza a través del EvaluadorDecisiones.
/// </summary>
public class ZonaPeligro : MonoBehaviour
{
    private static readonly List<ZonaPeligro> todas = new List<ZonaPeligro>();

    [Header("Configuración")]
    [Tooltip("Nombre que se muestra en subtítulos y reporte (ej. 'el fuego del pasillo este')")]
    [SerializeField] private string nombreZona = "una zona de fuego";
    [Tooltip("Radio de peligro inicial en metros")]
    [SerializeField] private float radio = 1.6f;
    [Tooltip("Segundos entre penalizaciones si el usuario permanece dentro")]
    [SerializeField] private float enfriamiento = 4f;

    [Header("Propagación del incendio")]
    [Tooltip("Radio de peligro cuando el incendio alcanza su tamaño máximo")]
    [SerializeField] private float radioFinal = 3.2f;
    [Tooltip("Segundos que tarda el fuego en crecer hasta su tamaño máximo")]
    [SerializeField] private float duracionPropagacion = 90f;
    [Tooltip("Multiplicador de emisión de partículas al tamaño máximo")]
    [SerializeField] private float factorEmisionFinal = 2.5f;

    private bool activa;
    private float proximaPenalizacion;
    private float tiempoActivacion;
    private Transform cabeza;

    private ParticleSystem[] sistemas;
    private float[] tasasBase;
    private Light[] luces;
    private float[] intensidadesBase;

    /// <summary>0 = recién encendido, 1 = incendio totalmente desarrollado.</summary>
    public float Propagacion { get; private set; }

    private void OnEnable() => todas.Add(this);
    private void OnDisable() => todas.Remove(this);

    private void Start()
    {
        if (Camera.main != null)
            cabeza = Camera.main.transform;

        // Cachea partículas y luces hijas con sus valores base.
        sistemas = GetComponentsInChildren<ParticleSystem>(true);
        tasasBase = new float[sistemas.Length];
        for (int i = 0; i < sistemas.Length; i++)
            tasasBase[i] = sistemas[i].emission.rateOverTime.constant;

        luces = GetComponentsInChildren<Light>(true);
        intensidadesBase = new float[luces.Length];
        for (int i = 0; i < luces.Length; i++)
            intensidadesBase[i] = luces[i].intensity;

        SetActiva(false);
    }

    private void Update()
    {
        if (!activa || cabeza == null)
            return;

        // --- Propagación: el incendio crece con el tiempo ---
        Propagacion = duracionPropagacion <= 0f
            ? 1f
            : Mathf.Clamp01((Time.time - tiempoActivacion) / duracionPropagacion);

        float radioActual = Mathf.Lerp(radio, radioFinal, Propagacion);
        float factorEmision = Mathf.Lerp(1f, factorEmisionFinal, Propagacion);

        for (int i = 0; i < sistemas.Length; i++)
        {
            var em = sistemas[i].emission;
            em.rateOverTime = tasasBase[i] * factorEmision;
        }
        for (int i = 0; i < luces.Length; i++)
        {
            // Parpadeo leve para simular llamas + crecimiento por propagación.
            float parpadeo = 1f + 0.15f * Mathf.Sin(Time.time * 9f + i);
            luces[i].intensity = intensidadesBase[i] * (0.8f + Propagacion) * parpadeo;
            luces[i].range = Mathf.Lerp(7f, 12f, Propagacion);
        }

        // --- Penalización por proximidad (distancia en el plano XZ) ---
        Vector3 delta = cabeza.position - transform.position;
        delta.y = 0f;

        if (delta.sqrMagnitude <= radioActual * radioActual && Time.time >= proximaPenalizacion)
        {
            proximaPenalizacion = Time.time + enfriamiento;
            if (EvaluadorDecisiones.Instance != null)
                EvaluadorDecisiones.Instance.RegistrarContactoPeligro(nombreZona);
        }
    }

    /// <summary>Enciende o apaga el incendio (partículas y luces hijas).</summary>
    public void SetActiva(bool valor)
    {
        activa = valor;
        proximaPenalizacion = 0f;
        Propagacion = 0f;
        if (valor)
            tiempoActivacion = Time.time;

        if (sistemas == null)
            sistemas = GetComponentsInChildren<ParticleSystem>(true);

        foreach (var ps in sistemas)
        {
            if (valor) ps.Play();
            else ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        if (luces != null)
            foreach (var luz in luces)
                luz.enabled = valor;
        else
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
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, radioFinal);
    }
}
