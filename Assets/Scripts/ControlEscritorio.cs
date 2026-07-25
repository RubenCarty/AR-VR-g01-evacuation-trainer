using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Modo escritorio: permite probar el simulacro SIN visor VR ni mandos.
/// Se instala solo al dar Play (RuntimeInitializeOnLoadMethod) sobre el
/// XR Origin, únicamente cuando no hay un casco XR activo.
///
/// Controles:
///   W A S D      → caminar (relativo a la mirada)
///   Shift        → correr
///   Clic derecho → mantener para mirar con el ratón
/// La detección de decisiones, peligros y salida funciona igual que en VR
/// porque todo se basa en la posición de la cámara.
/// </summary>
public class ControlEscritorio : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 3f;
    [SerializeField] private float velocidadCorrer = 5.5f;
    [SerializeField] private float sensibilidadRaton = 0.14f;
    [SerializeField] private float alturaOjos = 1.6f;

    private XROrigin origen;
    private CharacterController controlador;
    private Transform camara;
    private float pitch;
    private float velocidadY;

    /// <summary>Se agrega solo al XR Origin al cargar cualquier escena en Play.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInstalar()
    {
        var origin = Object.FindFirstObjectByType<XROrigin>();
        if (origin != null && origin.GetComponent<ControlEscritorio>() == null)
            origin.gameObject.AddComponent<ControlEscritorio>();
    }

    private void Start()
    {
        origen = GetComponent<XROrigin>();
        camara = origen != null && origen.Camera != null ? origen.Camera.transform : null;

        // Si hay un casco XR activo, este modo no debe interferir.
        if (XRSettings.isDeviceActive || camara == null)
        {
            enabled = false;
            return;
        }

        controlador = GetComponent<CharacterController>();
        if (controlador == null)
        {
            controlador = gameObject.AddComponent<CharacterController>();
            controlador.height = 1.7f;
            controlador.center = new Vector3(0f, 0.9f, 0f);
            controlador.radius = 0.3f;
        }

        // Sin casco, la cámara queda a ras de piso: se sube a la altura de los ojos.
        var offset = origen.CameraFloorOffsetObject;
        if (offset != null && offset.transform.localPosition.y < 0.5f)
            offset.transform.localPosition = new Vector3(0f, alturaOjos, 0f);

        if (AccesibilidadManager.Instance != null)
            AccesibilidadManager.Instance.MostrarSubtitulo(
                "Modo escritorio: WASD para caminar, Shift correr, clic derecho + ratón para mirar.",
                8f, esAlerta: false);
    }

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        var teclado = Keyboard.current;
        var raton = Mouse.current;
        if (teclado == null)
            return;

        // ---- Mirar (mantener clic derecho) ----
        if (raton != null && raton.rightButton.isPressed)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Vector2 delta = raton.delta.ReadValue() * sensibilidadRaton;

            transform.Rotate(0f, delta.x, 0f);                       // yaw: gira el rig
            pitch = Mathf.Clamp(pitch - delta.y, -80f, 80f);         // pitch: inclina la cámara
            camara.localRotation = Quaternion.Euler(pitch, camara.localEulerAngles.y, 0f);
        }
        else if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
        }

        // ---- Caminar ----
        float x = (teclado.dKey.isPressed ? 1f : 0f) - (teclado.aKey.isPressed ? 1f : 0f);
        float z = (teclado.wKey.isPressed ? 1f : 0f) - (teclado.sKey.isPressed ? 1f : 0f);

        Vector3 adelante = camara.forward; adelante.y = 0f; adelante.Normalize();
        Vector3 derecha = camara.right; derecha.y = 0f; derecha.Normalize();
        Vector3 direccion = (adelante * z + derecha * x).normalized;

        bool correr = teclado.leftShiftKey.isPressed || teclado.rightShiftKey.isPressed;
        float rapidez = correr ? velocidadCorrer : velocidad;

        // ---- Gravedad ----
        if (controlador.isGrounded && velocidadY < 0f)
            velocidadY = -1f;
        velocidadY += -9.81f * Time.deltaTime;

        controlador.Move((direccion * rapidez + Vector3.up * velocidadY) * Time.deltaTime);
#endif
    }
}
