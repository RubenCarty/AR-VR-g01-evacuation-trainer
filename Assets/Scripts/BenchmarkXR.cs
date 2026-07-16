using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Benchmark de rendimiento para VR. Mide FPS durante una ventana de 60 s
/// (tecla B para iniciar) y reporta: FPS promedio, mínimo, máximo y el
/// "1% low" (promedio del 1% de frames más lentos, la métrica que mejor
/// refleja los tirones en VR). Guarda el resultado en un .txt y en la consola.
/// </summary>
public class BenchmarkXR : MonoBehaviour
{
    [Tooltip("Duración de la medición en segundos")]
    [SerializeField] private float duracion = 60f;
    [Tooltip("Iniciar automáticamente la medición cuando suena la alarma")]
    [SerializeField] private bool autoIniciarConAlarma = true;

    private readonly List<float> tiemposFrame = new List<float>(8192);
    private bool midiendo;
    private float tiempoRestante;
    private bool yaAutoInicio;

    public bool Midiendo => midiendo;
    public string UltimoResultado { get; private set; } = "";

    private void Start()
    {
        if (autoIniciarConAlarma && GestorEvacuacion.Instance != null)
            GestorEvacuacion.Instance.OnCambioEstado += OnEstadoSimulacro;
    }

    private void OnDestroy()
    {
        if (GestorEvacuacion.Instance != null)
            GestorEvacuacion.Instance.OnCambioEstado -= OnEstadoSimulacro;
    }

    private void OnEstadoSimulacro(GestorEvacuacion.EstadoSimulacro estado)
    {
        if (estado == GestorEvacuacion.EstadoSimulacro.Alarma && !yaAutoInicio)
        {
            yaAutoInicio = true;
            IniciarMedicion();
        }
    }

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
            IniciarMedicion();
#endif

        if (!midiendo)
            return;

        // unscaledDeltaTime: el benchmark no debe verse afectado por timeScale.
        tiemposFrame.Add(Time.unscaledDeltaTime);
        tiempoRestante -= Time.unscaledDeltaTime;

        if (tiempoRestante <= 0f)
            FinalizarMedicion();
    }

    [ContextMenu("Iniciar medición")]
    public void IniciarMedicion()
    {
        if (midiendo) return;
        midiendo = true;
        tiempoRestante = duracion;
        tiemposFrame.Clear();
        Debug.Log($"[BenchmarkXR] Medición iniciada ({duracion:F0} s)...");
    }

    private void FinalizarMedicion()
    {
        midiendo = false;
        if (tiemposFrame.Count == 0) return;

        float suma = 0f, peor = 0f, mejor = float.MaxValue;
        foreach (float t in tiemposFrame)
        {
            suma += t;
            if (t > peor) peor = t;
            if (t < mejor) mejor = t;
        }

        float fpsPromedio = tiemposFrame.Count / suma;

        // 1% low: promedio del 1% de frames más lentos.
        var ordenados = new List<float>(tiemposFrame);
        ordenados.Sort((a, b) => b.CompareTo(a)); // descendente: más lentos primero
        int n1 = Mathf.Max(1, ordenados.Count / 100);
        float suma1 = 0f;
        for (int i = 0; i < n1; i++)
            suma1 += ordenados[i];
        float fps1Low = n1 / suma1;

        var sb = new StringBuilder();
        sb.AppendLine("===== BENCHMARK XR =====");
        sb.AppendLine($"Fecha: {System.DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"Frames medidos: {tiemposFrame.Count} en {suma:F1} s");
        sb.AppendLine($"FPS promedio: {fpsPromedio:F1}");
        sb.AppendLine($"FPS mínimo instantáneo: {1f / peor:F1}");
        sb.AppendLine($"FPS máximo instantáneo: {1f / mejor:F1}");
        sb.AppendLine($"1% low: {fps1Low:F1}");
        sb.AppendLine($"Veredicto VR (>=72 FPS objetivo): {(fpsPromedio >= 72f ? "CUMPLE" : "NO CUMPLE — aplicar optimizaciones")}");

        UltimoResultado = sb.ToString();
        Debug.Log($"[BenchmarkXR]\n{UltimoResultado}");

        try
        {
            string ruta = Path.Combine(Application.persistentDataPath, "benchmark_xr.txt");
            File.AppendAllText(ruta, UltimoResultado + "\n");
            Debug.Log($"[BenchmarkXR] Resultado guardado en: {ruta}");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[BenchmarkXR] No se pudo guardar el archivo: {e.Message}");
        }

        if (AccesibilidadManager.Instance != null)
            AccesibilidadManager.Instance.MostrarSubtitulo(
                $"Benchmark: {fpsPromedio:F0} FPS promedio, 1% low {fps1Low:F0}.",
                6f, esAlerta: false);
    }
}
