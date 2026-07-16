using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

/// <summary>
/// Capa de locomoción accesible sobre el sistema de teleportación de XRIT.
/// Permite teletransportar al usuario por código (reinicio del simulacro),
/// guarda el punto de inicio y expone el "modo comodidad" (viñeta de túnel)
/// para reducir el cybersickness.
/// </summary>
public class TeleportSystem : MonoBehaviour
{
    public static TeleportSystem Instance { get; private set; }

    [Header("Referencias (auto-asignadas si se dejan vacías)")]
    [SerializeField] private TeleportationProvider proveedor;
    [SerializeField] private XROrigin origen;

    [Header("Inicio del simulacro")]
    [Tooltip("Punto donde aparece el usuario al iniciar o reiniciar (Aula 201, piso 2)")]
    [SerializeField] private Transform puntoInicio;

    [Header("Confort / accesibilidad")]
    [Tooltip("Si está activo, la viñeta de túnel reduce el mareo al moverse")]
    [SerializeField] private bool modoComodidad = true;

    private TunnelingVignetteController vineta;

    public bool ModoComodidad => modoComodidad;

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
        if (origen == null)
            origen = FindFirstObjectByType<XROrigin>();
        if (proveedor == null)
            proveedor = FindFirstObjectByType<TeleportationProvider>();

        vineta = FindFirstObjectByType<TunnelingVignetteController>(FindObjectsInactive.Include);
        AplicarModoComodidad();

        // Coloca al usuario en el punto de inicio al arrancar la escena.
        VolverAlInicio();
    }

    /// <summary>Teletransporta al usuario a una posición y rotación en el mundo.</summary>
    public void TeleportarA(Vector3 posicion, Quaternion rotacion)
    {
        if (proveedor != null)
        {
            var solicitud = new TeleportRequest
            {
                destinationPosition = posicion,
                destinationRotation = rotacion,
                matchOrientation = MatchOrientation.TargetUpAndForward,
                requestTime = Time.time
            };
            proveedor.QueueTeleportRequest(solicitud);
        }
        else if (origen != null)
        {
            // Respaldo sin proveedor: mover el rig directamente.
            origen.transform.SetPositionAndRotation(posicion, rotacion);
        }

        // Aviso sonoro de accesibilidad: confirma el movimiento a usuarios con baja visión.
        if (AccesibilidadManager.Instance != null)
            AccesibilidadManager.Instance.ReproducirCueMovimiento();
    }

    /// <summary>Devuelve al usuario al punto de inicio del simulacro.</summary>
    public void VolverAlInicio()
    {
        if (puntoInicio == null)
        {
            Debug.LogWarning("[TeleportSystem] No hay punto de inicio asignado.");
            return;
        }
        TeleportarA(puntoInicio.position, puntoInicio.rotation);
    }

    /// <summary>Activa o desactiva la viñeta de confort (accesibilidad anti-mareo).</summary>
    public void SetModoComodidad(bool activo)
    {
        modoComodidad = activo;
        AplicarModoComodidad();

        if (AccesibilidadManager.Instance != null)
            AccesibilidadManager.Instance.MostrarSubtitulo(
                activo ? "Modo comodidad activado: viñeta anti-mareo encendida."
                       : "Modo comodidad desactivado.",
                4f, esAlerta: false);
    }

    public void AlternarModoComodidad() => SetModoComodidad(!modoComodidad);

    private void AplicarModoComodidad()
    {
        if (vineta != null)
            vineta.gameObject.SetActive(modoComodidad);
    }
}
