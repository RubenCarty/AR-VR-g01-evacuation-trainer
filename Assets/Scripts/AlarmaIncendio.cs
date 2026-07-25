using System.Collections;
using UnityEngine;

/// <summary>
/// Alarma contra incendios de la escuela. La sirena se genera por código
/// (AudioClip.Create con un barrido de frecuencia), por lo que no requiere
/// ningún asset de audio externo. Además hace parpadear las luces de
/// emergencia rojas que tenga como hijas.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AlarmaIncendio : MonoBehaviour
{
    [Header("Sirena generada por código")]
    [Tooltip("Frecuencia inferior del barrido en Hz")]
    [SerializeField] private float frecuenciaMin = 550f;
    [Tooltip("Frecuencia superior del barrido en Hz")]
    [SerializeField] private float frecuenciaMax = 950f;
    [Tooltip("Duración de un ciclo de sirena en segundos")]
    [SerializeField] private float duracionCiclo = 1.6f;
    [Range(0f, 1f)]
    [SerializeField] private float volumen = 0.55f;

    [Header("Luces de emergencia (hijas)")]
    [SerializeField] private float velocidadParpadeo = 4f;

    public bool Activa { get; private set; }

    private AudioSource fuente;
    private Light[] luces;
    private Coroutine parpadeo;

    private void Awake()
    {
        fuente = GetComponent<AudioSource>();
        fuente.loop = true;
        fuente.spatialBlend = 0.6f;   // parcialmente 3D: se oye en todo el edificio
        fuente.volume = volumen;
        fuente.playOnAwake = false;
        fuente.clip = GenerarSirena();

        luces = GetComponentsInChildren<Light>(true);
        foreach (var luz in luces)
            luz.enabled = false;
    }

    /// <summary>Genera un ciclo de sirena (barrido ascendente y descendente).</summary>
    private AudioClip GenerarSirena()
    {
        const int frecuenciaMuestreo = 44100;
        int muestras = Mathf.RoundToInt(frecuenciaMuestreo * duracionCiclo);
        var datos = new float[muestras];

        float fase = 0f;
        for (int i = 0; i < muestras; i++)
        {
            float t = (float)i / muestras;
            // Triángulo 0→1→0 para subir y bajar la frecuencia en un ciclo.
            float sube = t < 0.5f ? t * 2f : (1f - t) * 2f;
            float freq = Mathf.Lerp(frecuenciaMin, frecuenciaMax, sube);
            fase += 2f * Mathf.PI * freq / frecuenciaMuestreo;
            datos[i] = Mathf.Sin(fase) * 0.8f;
        }

        var clip = AudioClip.Create("SirenaGenerada", muestras, 1, frecuenciaMuestreo, false);
        clip.SetData(datos, 0);
        return clip;
    }

    /// <summary>Enciende la sirena y el parpadeo de luces de emergencia.</summary>
    public void Activar()
    {
        if (Activa) return;
        Activa = true;
        fuente.Play();
        if (parpadeo == null)
            parpadeo = StartCoroutine(ParpadearLuces());
    }

    /// <summary>Apaga la sirena y las luces.</summary>
    public void Detener()
    {
        Activa = false;
        fuente.Stop();
        if (parpadeo != null)
        {
            StopCoroutine(parpadeo);
            parpadeo = null;
        }
        foreach (var luz in luces)
            luz.enabled = false;
    }

    private IEnumerator ParpadearLuces()
    {
        while (Activa)
        {
            bool encendidas = Mathf.PingPong(Time.time * velocidadParpadeo, 2f) > 1f;
            foreach (var luz in luces)
                luz.enabled = encendidas;
            yield return null;
        }
    }
}
