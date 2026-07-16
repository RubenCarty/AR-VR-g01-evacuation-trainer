using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Registra cada decisión del usuario durante la evacuación (ruta correcta,
/// ascensor, zonas de fuego) y calcula la nota final sobre 20 puntos:
///   - Decisión incorrecta: -2 pts
///   - Contacto con zona de peligro: -3 pts
///   - Exceso sobre el tiempo objetivo: -1 pt por cada 30 s extra
/// El reporte final se muestra en el HUD.
/// </summary>
public class EvaluadorDecisiones : MonoBehaviour
{
    public struct Decision
    {
        public string descripcion;
        public bool correcta;
        public float tiempo;
    }

    public static EvaluadorDecisiones Instance { get; private set; }

    private readonly List<Decision> decisiones = new List<Decision>();
    private int contactosPeligro;

    public IReadOnlyList<Decision> Decisiones => decisiones;
    public int ContactosPeligro => contactosPeligro;
    public float NotaFinal { get; private set; } = -1f;
    public string UltimoReporte { get; private set; } = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>Llamado por cada PuntoDecision cuando el usuario pasa por él.</summary>
    public void RegistrarDecision(string descripcion, bool correcta)
    {
        float t = GestorEvacuacion.Instance != null ? GestorEvacuacion.Instance.TiempoEvacuacion : 0f;
        decisiones.Add(new Decision { descripcion = descripcion, correcta = correcta, tiempo = t });
        Debug.Log($"[Evaluador] {(correcta ? "CORRECTA" : "INCORRECTA")} ({t:F1}s): {descripcion}");

        if (AccesibilidadManager.Instance != null)
        {
            AccesibilidadManager.Instance.MostrarSubtitulo(
                correcta ? $"✔ Buena decisión: {descripcion}" : $"✘ Decisión incorrecta: {descripcion}",
                4f, esAlerta: !correcta);
        }
    }

    /// <summary>Llamado por ZonaPeligro cuando el usuario entra en fuego o humo.</summary>
    public void RegistrarContactoPeligro(string nombreZona)
    {
        contactosPeligro++;
        Debug.Log($"[Evaluador] Contacto con peligro #{contactosPeligro}: {nombreZona}");

        if (AccesibilidadManager.Instance != null)
            AccesibilidadManager.Instance.MostrarSubtitulo(
                $"¡Peligro! Entraste en {nombreZona}. Aléjate del fuego.",
                4f, esAlerta: true);
    }

    /// <summary>Calcula la nota final y arma el reporte de texto para el HUD.</summary>
    public string GenerarReporte(float tiempoEvacuacion, float tiempoObjetivo)
    {
        int correctas = 0, incorrectas = 0;
        foreach (var d in decisiones)
        {
            if (d.correcta) correctas++;
            else incorrectas++;
        }

        float nota = 20f;
        nota -= incorrectas * 2f;
        nota -= contactosPeligro * 3f;

        float exceso = Mathf.Max(0f, tiempoEvacuacion - tiempoObjetivo);
        nota -= Mathf.Floor(exceso / 30f);

        NotaFinal = Mathf.Clamp(nota, 0f, 20f);

        var sb = new StringBuilder();
        sb.AppendLine("== RESULTADO DEL SIMULACRO ==");
        sb.AppendLine($"Tiempo de evacuación: {tiempoEvacuacion:F1} s (objetivo {tiempoObjetivo:F0} s)");
        sb.AppendLine($"Decisiones correctas: {correctas}   Incorrectas: {incorrectas}");
        sb.AppendLine($"Contactos con peligro: {contactosPeligro}");
        sb.AppendLine($"NOTA FINAL: {NotaFinal:F1} / 20");
        sb.AppendLine(NotaFinal >= 17f ? "¡Evacuación excelente!"
                    : NotaFinal >= 13f ? "Buena evacuación, con detalles por mejorar."
                    : "Repite el simulacro: revisa la ruta segura.");

        UltimoReporte = sb.ToString();
        Debug.Log($"[Evaluador]\n{UltimoReporte}");
        return UltimoReporte;
    }

    /// <summary>Limpia el historial para un nuevo intento.</summary>
    public void Reiniciar()
    {
        decisiones.Clear();
        contactosPeligro = 0;
        NotaFinal = -1f;
        UltimoReporte = "";
    }
}
