using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Conveniencia de primer arranque y de actualización: si la escena
/// EscuelaEvacuacion no existe, o si fue generada con una versión anterior
/// del constructor (marca en EscuelaEvacuacion.version.txt), la (re)genera
/// automáticamente una sola vez por sesión de editor.
/// La reconstrucción manual siempre está disponible en el menú Herramientas.
/// </summary>
[InitializeOnLoad]
public static class AutoConstruirEscena
{
    private const string RutaEscena = "Assets/Scenes/EscuelaEvacuacion.unity";
    private const string RutaVersion = "Assets/Scenes/EscuelaEvacuacion.version.txt";
    private static readonly string ClaveSesion = $"EvacuationTrainer.AutoConstruir.v{ConstructorEscuela.VersionEscena}";

    static AutoConstruirEscena()
    {
        if (SessionState.GetBool(ClaveSesion, false))
            return;
        if (EscenaActualizada())
            return;

        EditorApplication.update += IntentarConstruir;
    }

    private static bool EscenaActualizada()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(RutaEscena) == null)
            return false;
        if (!File.Exists(RutaVersion))
            return false;

        return int.TryParse(File.ReadAllText(RutaVersion).Trim(), out int version)
               && version >= ConstructorEscuela.VersionEscena;
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

        if (EscenaActualizada())
            return;

        Debug.Log($"[AutoConstruirEscena] Generando la escena EscuelaEvacuacion (versión {ConstructorEscuela.VersionEscena})...");
        ConstructorEscuela.Construir();
    }
}
