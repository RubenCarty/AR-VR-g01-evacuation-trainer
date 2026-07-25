using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Centro de accesibilidad del entrenador (lo que hace "inclusivo" al proyecto):
///  - Subtítulos de todos los eventos sonoros (alarma, avisos) con fondo legible.
///  - Modo alto contraste (texto amarillo sobre negro, señales más brillantes).
///  - Escala de texto ajustable para usuarios con baja visión.
///  - Cues de audio: beeps distintos para alertas y confirmación de teleport.
/// Atajos de demo: C = alto contraste, +/- = tamaño de texto.
/// </summary>
public class AccesibilidadManager : MonoBehaviour
{
    public static AccesibilidadManager Instance { get; private set; }

    [Header("Subtítulos (asignados por el constructor de escena)")]
    [SerializeField] private TextMeshProUGUI textoSubtitulo;
    [SerializeField] private Image fondoSubtitulo;

    [Header("Preferencias")]
    [SerializeField] private bool subtitulosActivos = true;
    [SerializeField] private bool altoContraste = false;
    [Range(0.7f, 1.8f)]
    [SerializeField] private float escalaTexto = 1f;
    [SerializeField] private bool cuesDeAudio = true;

    private float tamanoBase = 28f;
    private AudioSource fuenteCues;
    private AudioClip beepAlerta;
    private AudioClip beepInfo;
    private AudioClip beepTeleport;
    private Coroutine subtituloActual;

    public bool AltoContraste => altoContraste;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        fuenteCues = gameObject.AddComponent<AudioSource>();
        fuenteCues.playOnAwake = false;
        fuenteCues.spatialBlend = 0f; // los cues siempre se oyen claros (2D)

        beepAlerta = GenerarBeep(880f, 0.18f, 3);   // triple beep agudo = alerta
        beepInfo = GenerarBeep(520f, 0.12f, 1);     // beep suave = información
        beepTeleport = GenerarBeep(660f, 0.07f, 1); // clic corto = confirmación
    }

    private void Start()
    {
        if (textoSubtitulo != null)
            tamanoBase = textoSubtitulo.fontSize;
        AplicarEstilo();
        if (textoSubtitulo != null)
            textoSubtitulo.text = "";
    }

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.cKey.wasPressedThisFrame)
            AlternarAltoContraste();
        if (kb.equalsKey.wasPressedThisFrame || kb.numpadPlusKey.wasPressedThisFrame)
            CambiarEscalaTexto(0.1f);
        if (kb.minusKey.wasPressedThisFrame || kb.numpadMinusKey.wasPressedThisFrame)
            CambiarEscalaTexto(-0.1f);
#endif
    }

    /// <summary>Muestra un subtítulo con duración fija y cue de audio asociado.</summary>
    public void MostrarSubtitulo(string mensaje, float duracion, bool esAlerta)
    {
        if (cuesDeAudio && fuenteCues != null)
            fuenteCues.PlayOneShot(esAlerta ? beepAlerta : beepInfo, esAlerta ? 0.9f : 0.5f);

        if (!subtitulosActivos || textoSubtitulo == null)
            return;

        if (subtituloActual != null)
            StopCoroutine(subtituloActual);
        subtituloActual = StartCoroutine(RutinaSubtitulo(mensaje, duracion));
    }

    private IEnumerator RutinaSubtitulo(string mensaje, float duracion)
    {
        textoSubtitulo.text = mensaje;
        if (fondoSubtitulo != null)
            fondoSubtitulo.enabled = true;
        yield return new WaitForSeconds(duracion);
        textoSubtitulo.text = "";
        if (fondoSubtitulo != null)
            fondoSubtitulo.enabled = false;
        subtituloActual = null;
    }

    /// <summary>Cue corto que confirma cada teletransporte (baja visión).</summary>
    public void ReproducirCueMovimiento()
    {
        if (cuesDeAudio && fuenteCues != null)
            fuenteCues.PlayOneShot(beepTeleport, 0.4f);
    }

    public void AlternarAltoContraste()
    {
        altoContraste = !altoContraste;
        AplicarEstilo();
        MostrarSubtitulo(altoContraste ? "Alto contraste ACTIVADO." : "Alto contraste desactivado.",
            3f, esAlerta: false);
    }

    public void CambiarEscalaTexto(float delta)
    {
        escalaTexto = Mathf.Clamp(escalaTexto + delta, 0.7f, 1.8f);
        AplicarEstilo();
    }

    private void AplicarEstilo()
    {
        if (textoSubtitulo != null)
        {
            textoSubtitulo.fontSize = tamanoBase * escalaTexto;
            textoSubtitulo.color = altoContraste ? Color.yellow : Color.white;
        }
        if (fondoSubtitulo != null)
            fondoSubtitulo.color = altoContraste
                ? new Color(0f, 0f, 0f, 0.95f)
                : new Color(0f, 0f, 0f, 0.6f);
    }

    /// <summary>Genera un beep (o serie de beeps) por código, sin assets externos.</summary>
    private AudioClip GenerarBeep(float frecuencia, float duracionBeep, int repeticiones)
    {
        const int fs = 44100;
        float silencio = 0.08f;
        int muestrasBeep = Mathf.RoundToInt(fs * duracionBeep);
        int muestrasSilencio = Mathf.RoundToInt(fs * silencio);
        int total = repeticiones * muestrasBeep + (repeticiones - 1) * muestrasSilencio;
        var datos = new float[total];

        int indice = 0;
        for (int r = 0; r < repeticiones; r++)
        {
            for (int i = 0; i < muestrasBeep; i++)
            {
                // Envolvente simple para evitar clics al inicio y final.
                float env = Mathf.Min(1f, Mathf.Min(i, muestrasBeep - i) / (fs * 0.01f));
                datos[indice++] = Mathf.Sin(2f * Mathf.PI * frecuencia * i / fs) * 0.6f * env;
            }
            if (r < repeticiones - 1)
                indice += muestrasSilencio;
        }

        var clip = AudioClip.Create($"Beep{frecuencia:F0}x{repeticiones}", total, 1, fs, false);
        clip.SetData(datos, 0);
        return clip;
    }

    /// <summary>Permite al constructor de escena conectar la UI de subtítulos.</summary>
    public void ConectarUI(TextMeshProUGUI texto, Image fondo)
    {
        textoSubtitulo = texto;
        fondoSubtitulo = fondo;
        if (textoSubtitulo != null)
            tamanoBase = textoSubtitulo.fontSize;
        AplicarEstilo();
    }
}
