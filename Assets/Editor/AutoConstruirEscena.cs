using UnityEditor;
using UnityEngine;

/// <summary>
/// Conveniencia de primer arranque: si la escena EscuelaEvacuacion aún no
/// existe, la construye automáticamente (una sola vez por sesión de editor)
/// llamando a ConstructorEscuela. Después de generada, este script no vuelve
/// a hacer nada; la reconstrucción manual queda en el menú Herramientas.
/// </summary>
[InitializeOnLoad]
public static class AutoConstruirEscena
{
    private const string RutaEscena = "Assets/Scenes/EscuelaEvacuacion.unity";
    private const string ClaveSesion = "EvacuationTrainer.AutoConstruirIntentado";

    static AutoConstruirEscena()
    {
        if (SessionState.GetBool(ClaveSesion, false))
            return;
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(RutaEscena) != null)
            return;

        EditorApplication.update += IntentarConstruir;
    }

    private static void IntentarConstruir()
    {
        // Espera a que el editor esté libre (sin compilar, importar ni entrar a Play).
        if (EditorApplication.isCompiling ||
            EditorApplication.isUpdating ||
            EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        EditorApplication.update -= IntentarConstruir;
        SessionState.SetBool(ClaveSesion, true);

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(RutaEscena) != null)
            return;

        Debug.Log("[AutoConstruirEscena] Generando la escena EscuelaEvacuacion por primera vez...");
        ConstructorEscuela.Construir();
    }
}
