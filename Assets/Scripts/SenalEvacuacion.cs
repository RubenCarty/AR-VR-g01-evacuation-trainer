using UnityEngine;

/// <summary>
/// Señal luminosa de evacuación (cartel "SALIDA →"). Pulsa suavemente su
/// emisión para atraer la mirada y aumenta su brillo cuando el
/// AccesibilidadManager tiene el modo alto contraste activo.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class SenalEvacuacion : MonoBehaviour
{
    [SerializeField] private Color colorEmision = new Color(0.1f, 1f, 0.3f);
    [Tooltip("Velocidad del pulso de brillo")]
    [SerializeField] private float velocidadPulso = 2f;
    [Tooltip("Brillo base de la señal")]
    [SerializeField] private float intensidadBase = 1.5f;

    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    private Material material;

    private void Start()
    {
        // Instancia propia del material para no alterar las demás señales.
        material = GetComponent<Renderer>().material;
        material.EnableKeyword("_EMISSION");
    }

    private void Update()
    {
        if (material == null) return;

        float pulso = 0.75f + 0.25f * Mathf.Sin(Time.time * velocidadPulso);
        float contraste = (AccesibilidadManager.Instance != null &&
                           AccesibilidadManager.Instance.AltoContraste) ? 2.2f : 1f;

        material.SetColor(EmissionColor, colorEmision * intensidadBase * pulso * contraste);
    }
}
