using UnityEngine;

public class ControlAscensor : MonoBehaviour
{
    [Header("Referencia a la Interfaz (UI)")]
    public GameObject panelBotonera; // Aquí arrastras tu Panel_Control_Nuevo

    // Esta función se ejecuta automáticamente cuando el personaje entra al ascensor
    private void OnTriggerEnter(Collider other)
    {
        // Comprobamos si el objeto que entró es tu personaje
        if (other.name.Contains("Personaje") || other.CompareTag("Player"))
        {
            // 1. Activamos el panel de botones
            if (panelBotonera != null)
            {
                panelBotonera.SetActive(true);
            }

            // 2. Apagamos el script de movimiento del personaje para congelarlo y liberar la cámara
            LogicaPersonaje1 scriptMovimiento = other.GetComponent<LogicaPersonaje1>();
            if (scriptMovimiento != null)
            {
                scriptMovimiento.enabled = false;
            }

            // 3. Liberamos el mouse en pantalla
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Esta función se ejecuta cuando el personaje sale (o cuando decidas cerrar la interfaz)
    private void OnTriggerExit(Collider other)
    {
        if (other.name.Contains("Personaje") || other.CompareTag("Player"))
        {
            // 1. Ocultamos el panel
            if (panelBotonera != null)
            {
                panelBotonera.SetActive(false);
            }

            // 2. Volvemos a encender el script de movimiento de tu personaje
            LogicaPersonaje1 scriptMovimiento = other.GetComponent<LogicaPersonaje1>();
            if (scriptMovimiento != null)
            {
                scriptMovimiento.enabled = true;
            }

            // 3. Volvemos a ocultar y bloquear el mouse para jugar normal
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Puedes llamar a este método desde el botón del piso para reactivar al jugador después de moverse
    public void ReactivarJugador(GameObject jugador)
    {
        if (panelBotonera != null)
        {
            panelBotonera.SetActive(false);
        }

        LogicaPersonaje1 scriptMovimiento = jugador.GetComponent<LogicaPersonaje1>();
        if (scriptMovimiento != null)
        {
            scriptMovimiento.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}