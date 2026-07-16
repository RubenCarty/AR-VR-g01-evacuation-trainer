using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

/// <summary>
/// Constructor procedural de la escena "EscuelaEvacuacion": una escuela de
/// 2 pisos con aulas, pasillos, escalera de emergencia, ascensor (ruta
/// incorrecta), zonas de fuego, señalización de evacuación, áreas de
/// teleportación XRIT, XR Origin, HUD accesible y todos los gestores.
///
/// Uso: menú  Herramientas → Evacuation Trainer → Construir Escena Escuela (2 pisos)
/// El script es re-ejecutable: sobrescribe la escena generada.
/// </summary>
public static class ConstructorEscuela
{
    private const string RutaEscena = "Assets/Scenes/EscuelaEvacuacion.unity";
    private const string CarpetaMateriales = "Assets/Materiales";
    private const string RutaRig = "Assets/Samples/XR Interaction Toolkit/3.4.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";
    private const string RutaAreaTeleport = "Assets/Samples/XR Interaction Toolkit/3.4.1/Starter Assets/DemoAssets/Prefabs/Teleport/Teleport Area.prefab";
    private const string RutaAnclaTeleport = "Assets/Samples/XR Interaction Toolkit/3.4.1/Starter Assets/DemoAssets/Prefabs/Teleport/Teleport Anchor.prefab";
    private const string RutaMatParticulas = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/ParticlesUnlit.mat";

    // Dimensiones del edificio
    private const float AltoPiso = 3.3f;    // altura libre de cada piso
    private const float GrosorLosa = 0.2f;  // losa entre pisos
    private const float PisoDos = 3.4f;     // cota superior de la losa del piso 2

    // Materiales compartidos durante la construcción
    private static Material matPared, matParedInterior, matPisoPasillo, matPisoAula,
        matLosa, matTecho, matPuerta, matMadera, matPizarra, matSenal, matSenalRojo,
        matMetal, matVerde, matLampara;

    [MenuItem("Herramientas/Evacuation Trainer/Construir Escena Escuela (2 pisos)")]
    public static void Construir()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        CrearMateriales();

        var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        ConfigurarIluminacionGlobal();

        var edificio = new GameObject("== EDIFICIO ESCUELA ==").transform;
        ConstruirTerrenoYPatio(edificio);
        ConstruirPiso1(edificio);
        ConstruirPiso2(edificio);
        ConstruirEscalera(edificio);
        ConstruirAscensor(edificio);
        ConstruirSenales(edificio);
        ConstruirMobiliario(edificio);
        ConstruirLamparas(edificio);

        var jugabilidad = new GameObject("== JUGABILIDAD ==").transform;
        ConstruirZonasPeligro(jugabilidad);
        ConstruirPuntosDecision(jugabilidad);
        ConstruirTeleportacion(jugabilidad);

        var puntoInicio = new GameObject("PuntoInicio").transform;
        puntoInicio.SetParent(jugabilidad);
        puntoInicio.SetPositionAndRotation(new Vector3(-11.5f, PisoDos + 0.02f, -6.3f), Quaternion.Euler(0f, 35f, 0f));

        var rig = ColocarRigXR(puntoInicio);
        var hud = ConstruirHUD();
        ConstruirGestores(jugabilidad, puntoInicio, hud);

        EditorSceneManager.SaveScene(escena, RutaEscena);
        AgregarABuildSettings();

        Debug.Log($"[ConstructorEscuela] Escena generada y guardada en {RutaEscena}. " +
                  "Pulsa Play: apareces en el Aula 201 (piso 2); la alarma suena a los 12 s.");
        if (rig == null)
            Debug.LogWarning("[ConstructorEscuela] No se encontró el prefab del XR Origin. Revisa la ruta de los samples de XRIT.");
    }

    // ------------------------------------------------------------------
    //  Materiales
    // ------------------------------------------------------------------
    private static void CrearMateriales()
    {
        if (!AssetDatabase.IsValidFolder(CarpetaMateriales))
            AssetDatabase.CreateFolder("Assets", "Materiales");

        matPared         = Mat("Pared_Exterior",  new Color(0.85f, 0.82f, 0.74f));
        matParedInterior = Mat("Pared_Interior",  new Color(0.93f, 0.91f, 0.86f));
        matPisoPasillo   = Mat("Piso_Pasillo",    new Color(0.55f, 0.62f, 0.68f), 0.05f, 0.6f);
        matPisoAula      = Mat("Piso_Aula",       new Color(0.72f, 0.55f, 0.38f), 0f, 0.45f);
        matLosa          = Mat("Losa",            new Color(0.75f, 0.74f, 0.72f));
        matTecho         = Mat("Techo",           new Color(0.45f, 0.45f, 0.48f));
        matPuerta        = Mat("Puerta",          new Color(0.42f, 0.26f, 0.15f), 0f, 0.5f);
        matMadera        = Mat("Madera",          new Color(0.6f, 0.42f, 0.24f), 0f, 0.4f);
        matPizarra       = Mat("Pizarra",         new Color(0.1f, 0.3f, 0.22f), 0f, 0.3f);
        matSenal         = Mat("Senal_Verde",     new Color(0.05f, 0.35f, 0.12f), 0f, 0.5f, new Color(0.1f, 1f, 0.3f) * 1.4f);
        matSenalRojo     = Mat("Senal_Roja",      new Color(0.4f, 0.05f, 0.05f), 0f, 0.5f, new Color(1f, 0.12f, 0.1f) * 1.2f);
        matMetal         = Mat("Metal",           new Color(0.6f, 0.62f, 0.65f), 0.7f, 0.7f);
        matVerde         = Mat("Cesped",          new Color(0.33f, 0.52f, 0.27f), 0f, 0.15f);
        matLampara       = Mat("Lampara",         Color.white, 0f, 0.5f, new Color(1f, 0.98f, 0.9f) * 1.6f);
    }

    /// <summary>Crea (o reutiliza) un material URP Lit guardado como asset.</summary>
    private static Material Mat(string nombre, Color color, float metallic = 0f, float smooth = 0.35f, Color? emision = null)
    {
        string ruta = $"{CarpetaMateriales}/{nombre}.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, ruta);
        }
        m.SetColor("_BaseColor", color);
        m.SetFloat("_Metallic", metallic);
        m.SetFloat("_Smoothness", smooth);
        if (emision.HasValue)
        {
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            m.SetColor("_EmissionColor", emision.Value);
        }
        EditorUtility.SetDirty(m);
        return m;
    }

    // ------------------------------------------------------------------
    //  Iluminación
    // ------------------------------------------------------------------
    private static void ConfigurarIluminacionGlobal()
    {
        var sol = new GameObject("Luz Direccional (Sol)");
        var luz = sol.AddComponent<Light>();
        luz.type = LightType.Directional;
        luz.intensity = 1.25f;
        luz.color = new Color(1f, 0.96f, 0.88f);
        luz.shadows = LightShadows.Soft;
        sol.transform.rotation = Quaternion.Euler(55f, -35f, 0f);

        // Ambiente plano para que el interior sea legible sin GI horneada.
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.45f, 0.46f, 0.5f);
    }

    private static void ConstruirLamparas(Transform padre)
    {
        var grupo = Grupo("Lamparas", padre);
        float[] xs = { -10f, -5f, 0f, 5f, 10f };
        foreach (float x in xs)
        {
            // Plafones emisivos bajo la losa (piso 1) y bajo el techo (piso 2).
            Caja($"Plafon P1 {x}", new Vector3(x, AltoPiso - 0.02f, 0f), new Vector3(1.4f, 0.04f, 0.5f), matLampara, grupo);
            Caja($"Plafon P2 {x}", new Vector3(x, PisoDos + AltoPiso - 0.02f, 0f), new Vector3(1.4f, 0.04f, 0.5f), matLampara, grupo);
        }
        float[] xsLuces = { -8f, 0f, 8f };
        foreach (float x in xsLuces)
        {
            Foco(grupo, new Vector3(x, AltoPiso - 0.5f, 0f));
            Foco(grupo, new Vector3(x, PisoDos + AltoPiso - 0.5f, 0f));
        }
    }

    private static void Foco(Transform padre, Vector3 pos)
    {
        var go = new GameObject($"Foco {pos.x}x{pos.y:F0}");
        go.transform.SetParent(padre);
        go.transform.position = pos;
        var l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.range = 9f;
        l.intensity = 1.1f;
        l.color = new Color(1f, 0.97f, 0.9f);
        l.shadows = LightShadows.None;
    }

    // ------------------------------------------------------------------
    //  Terreno, patio y estructura
    // ------------------------------------------------------------------
    private static void ConstruirTerrenoYPatio(Transform padre)
    {
        var grupo = Grupo("Terreno y Patio", padre);
        Caja("Terreno", new Vector3(0f, -0.1f, 0f), new Vector3(70f, 0.2f, 40f), matVerde, grupo);

        // Vereda de acceso desde la puerta principal hasta el punto de encuentro.
        Caja("Vereda", new Vector3(-17f, 0.005f, 0f), new Vector3(6f, 0.02f, 3f), matPisoPasillo, grupo);

        // Punto de encuentro (zona segura) en el patio oeste.
        var circulo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        circulo.name = "Circulo Punto de Encuentro";
        circulo.transform.SetParent(grupo);
        circulo.transform.position = new Vector3(-19f, 0.02f, 0f);
        circulo.transform.localScale = new Vector3(5f, 0.02f, 5f);
        circulo.GetComponent<Renderer>().sharedMaterial = matSenal;
        Object.DestroyImmediate(circulo.GetComponent<Collider>());

        var poste = Caja("Poste Encuentro", new Vector3(-19f, 1.1f, -2.6f), new Vector3(0.08f, 2.2f, 0.08f), matMetal, grupo);
        Senal("PUNTO DE\nENCUENTRO", new Vector3(-19f, 2.1f, -2.55f), 0f, grupo, matSenal, 1.4f, 0.9f);

        var salida = new GameObject("Zona Segura (SalidaSegura)");
        salida.transform.SetParent(grupo);
        salida.transform.position = new Vector3(-19f, 0f, 0f);
        salida.AddComponent<SalidaSegura>();
    }

    private static void ConstruirPiso1(Transform padre)
    {
        var g = Grupo("Piso 1", padre);

        // Pisos: pasillo central + aulas
        Caja("Piso Pasillo P1", new Vector3(0f, -0.05f, 0f), new Vector3(28f, 0.1f, 3.2f), matPisoPasillo, g);
        Caja("Piso Aulas Sur P1", new Vector3(0f, -0.051f, -4.85f), new Vector3(28f, 0.1f, 6.7f), matPisoAula, g);
        Caja("Piso Aulas Norte P1", new Vector3(0f, -0.051f, 4.85f), new Vector3(28f, 0.1f, 6.7f), matPisoAula, g);

        // Paredes exteriores (piso 1)
        ParedX(g, matPared, -8f, -14f, 14f, 0f, AltoPiso);                    // sur
        ParedX(g, matPared, 8f, -14f, 14f, 0f, AltoPiso);                     // norte
        ParedZ(g, matPared, 14f, -8f, 8f, 0f, AltoPiso);                      // este
        ParedZ(g, matPared, -14f, -8f, 8f, 0f, AltoPiso, 0f, 1.8f, 2.4f);     // oeste con puerta principal

        // Pared sur del pasillo, con puertas a las aulas del sur
        ParedX(g, matParedInterior, -1.6f, -14f, -6f, 0f, AltoPiso, -10f);    // Aula 101
        ParedX(g, matParedInterior, -1.6f, -6f, 2f, 0f, AltoPiso, -2f);       // Aula 102
        ParedX(g, matParedInterior, -1.6f, 2f, 14f, 0f, AltoPiso, 8f);        // Laboratorio

        // Pared norte del pasillo (abierta hacia el hall de la escalera al este)
        ParedX(g, matParedInterior, 1.6f, -14f, -4f, 0f, AltoPiso, -9f);      // Aula 103
        ParedX(g, matParedInterior, 1.6f, -4f, 6f, 0f, AltoPiso, 0f);         // Dirección

        // Divisores de aulas
        ParedZ(g, matParedInterior, -6f, -8f, -1.6f, 0f, AltoPiso);
        ParedZ(g, matParedInterior, 2f, -8f, -1.6f, 0f, AltoPiso);
        ParedZ(g, matParedInterior, -4f, 1.6f, 8f, 0f, AltoPiso);
        ParedZ(g, matParedInterior, 4f, 1.6f, 8f, 0f, AltoPiso);              // separa Dirección del hall escalera
    }

    private static void ConstruirPiso2(Transform padre)
    {
        var g = Grupo("Piso 2", padre);
        float y0 = PisoDos;

        // Losa del piso 2 en piezas, dejando el hueco de la escalera (x 8..12.9, z 2.5..6.5)
        Caja("Losa A", new Vector3(-3f, PisoDos - GrosorLosa / 2f, 0f), new Vector3(22f, GrosorLosa, 16f), matLosa, g);        // x -14..8
        Caja("Losa B", new Vector3(11f, PisoDos - GrosorLosa / 2f, -2.75f), new Vector3(6f, GrosorLosa, 10.5f), matLosa, g);   // x 8..14, z -8..2.5
        Caja("Losa C", new Vector3(11f, PisoDos - GrosorLosa / 2f, 7.25f), new Vector3(6f, GrosorLosa, 1.5f), matLosa, g);     // x 8..14, z 6.5..8
        Caja("Losa D (descanso)", new Vector3(13.45f, PisoDos - GrosorLosa / 2f, 4.5f), new Vector3(1.1f, GrosorLosa, 4f), matLosa, g); // x 12.9..14

        // Acabado de piso sobre la losa
        Caja("Piso Pasillo P2", new Vector3(0f, y0 + 0.005f, 0f), new Vector3(28f, 0.01f, 3.2f), matPisoPasillo, g);
        Caja("Piso Aulas Sur P2", new Vector3(0f, y0 + 0.004f, -4.85f), new Vector3(28f, 0.01f, 6.7f), matPisoAula, g);
        Caja("Piso Aulas Norte P2", new Vector3(-5f, y0 + 0.004f, 4.85f), new Vector3(18f, 0.01f, 6.7f), matPisoAula, g);

        // Paredes exteriores (piso 2)
        ParedX(g, matPared, -8f, -14f, 14f, y0, AltoPiso);
        ParedX(g, matPared, 8f, -14f, 14f, y0, AltoPiso);
        ParedZ(g, matPared, 14f, -8f, 8f, y0, AltoPiso);
        ParedZ(g, matPared, -14f, -8f, 8f, y0, AltoPiso);

        // Pared sur del pasillo, con puertas
        ParedX(g, matParedInterior, -1.6f, -14f, -6f, y0, AltoPiso, -10f);    // Aula 201 (inicio)
        ParedX(g, matParedInterior, -1.6f, -6f, 2f, y0, AltoPiso, -2f);       // Aula 202
        ParedX(g, matParedInterior, -1.6f, 2f, 14f, y0, AltoPiso, 8f);        // Biblioteca

        // Pared norte del pasillo (abierta hacia el hall de la escalera)
        ParedX(g, matParedInterior, 1.6f, -14f, -4f, y0, AltoPiso, -9f);      // Aula 203
        ParedX(g, matParedInterior, 1.6f, -4f, 6f, y0, AltoPiso, 0f);         // Sala de Profesores

        // Divisores
        ParedZ(g, matParedInterior, -6f, -8f, -1.6f, y0, AltoPiso);
        ParedZ(g, matParedInterior, 2f, -8f, -1.6f, y0, AltoPiso);
        ParedZ(g, matParedInterior, -4f, 1.6f, 8f, y0, AltoPiso);
        ParedZ(g, matParedInterior, 4f, 1.6f, 8f, y0, AltoPiso);

        // Techo del edificio
        Caja("Techo", new Vector3(0f, PisoDos + AltoPiso + 0.1f, 0f), new Vector3(28.4f, 0.2f, 16.4f), matTecho, g);

        // Barandas alrededor del hueco de la escalera
        Caja("Baranda Oeste", new Vector3(8f, y0 + 0.5f, 4.5f), new Vector3(0.08f, 1f, 4f), matMetal, g);
        Caja("Baranda Sur", new Vector3(10.45f, y0 + 0.5f, 2.55f), new Vector3(4.9f, 1f, 0.08f), matMetal, g);
        Caja("Baranda Norte", new Vector3(10.45f, y0 + 0.5f, 6.45f), new Vector3(4.9f, 1f, 0.08f), matMetal, g);
    }

    private static void ConstruirEscalera(Transform padre)
    {
        var g = Grupo("Escalera de Emergencia", padre);

        // Rampa sólida (los teleports usan anclas, la rampa da la lectura visual)
        float x0 = 8.5f, x1 = 12.9f, alto = PisoDos;
        float run = x1 - x0;
        float largo = Mathf.Sqrt(run * run + alto * alto);
        float angulo = Mathf.Atan2(alto, run) * Mathf.Rad2Deg;

        var rampa = Caja("Rampa Escalera", new Vector3((x0 + x1) / 2f, alto / 2f, 4.5f),
            new Vector3(largo + 0.3f, 0.18f, 2.8f), matPisoPasillo, g);
        rampa.transform.rotation = Quaternion.Euler(0f, 0f, angulo);

        // Contrahuellas decorativas para que se lea como escalera
        int pasos = 10;
        for (int i = 0; i < pasos; i++)
        {
            float t = (i + 0.5f) / pasos;
            Caja($"Peldano {i + 1}",
                new Vector3(Mathf.Lerp(x0, x1, t), Mathf.Lerp(0f, alto, t) + 0.02f, 4.5f),
                new Vector3(0.45f, 0.06f, 2.8f), matMetal, g);
        }

        // Barandas laterales de la rampa
        foreach (float z in new[] { 3.15f, 5.85f })
        {
            var baranda = Caja($"Baranda Rampa z{z}", new Vector3((x0 + x1) / 2f, alto / 2f + 0.55f, z),
                new Vector3(largo + 0.3f, 0.9f, 0.07f), matMetal, g);
            baranda.transform.rotation = Quaternion.Euler(0f, 0f, angulo);
        }
    }

    private static void ConstruirAscensor(Transform padre)
    {
        var g = Grupo("Ascensor", padre);

        // Caja del ascensor en el hall noreste, ambos pisos
        Caja("Cabina Ascensor", new Vector3(5.5f, (PisoDos + AltoPiso) / 2f, 7.2f),
            new Vector3(1.9f, PisoDos + AltoPiso, 1.6f), matMetal, g);

        // Puertas (frente sur) en cada piso
        Caja("Puerta Ascensor P1", new Vector3(5.5f, 1.1f, 6.38f), new Vector3(1.3f, 2.2f, 0.06f), matPuerta, g);
        Caja("Puerta Ascensor P2", new Vector3(5.5f, PisoDos + 1.1f, 6.38f), new Vector3(1.3f, 2.2f, 0.06f), matPuerta, g);
    }

    // ------------------------------------------------------------------
    //  Señalización
    // ------------------------------------------------------------------
    private static void ConstruirSenales(Transform padre)
    {
        var g = Grupo("Senales de Evacuacion", padre);

        // Piso 2: guían hacia el este (escalera)
        Senal("SALIDA →", new Vector3(-6f, PisoDos + 2.6f, 1.45f), 180f, g, matSenal);
        Senal("SALIDA →", new Vector3(0f, PisoDos + 2.6f, 1.45f), 180f, g, matSenal);
        Senal("ESCALERA DE\nEMERGENCIA →", new Vector3(5f, PisoDos + 2.6f, 1.45f), 180f, g, matSenal, 1.5f, 0.7f);
        Senal("↓ BAJA POR\nLA ESCALERA", new Vector3(13.75f, PisoDos + 2.4f, 4.5f), -90f, g, matSenal, 1.5f, 0.7f);
        Senal("NO USAR EN\nCASO DE INCENDIO", new Vector3(5.5f, PisoDos + 2.5f, 6.34f), 180f, g, matSenalRojo, 1.5f, 0.7f);

        // Piso 1: guían hacia el oeste (puerta principal)
        Senal("← SALIDA", new Vector3(6f, 2.6f, 1.45f), 180f, g, matSenal);
        Senal("← SALIDA", new Vector3(0f, 2.6f, 1.45f), 180f, g, matSenal);
        Senal("← SALIDA", new Vector3(-7f, 2.6f, 1.45f), 180f, g, matSenal);
        Senal("SALIDA", new Vector3(-13.85f, 2.75f, 0f), 90f, g, matSenal);
        Senal("NO USAR EN\nCASO DE INCENDIO", new Vector3(5.5f, 2.5f, 6.34f), 180f, g, matSenalRojo, 1.5f, 0.7f);

        // Cartel informativo en el aula de inicio
        Senal("SIMULACRO DE EVACUACIÓN\nSigue las señales verdes", new Vector3(-10f, PisoDos + 2.2f, -7.9f), 0f, g, matSenal, 2.6f, 0.9f);
    }

    /// <summary>Cartel emisivo con texto TMP. rotY = dirección hacia la que mira.</summary>
    private static GameObject Senal(string texto, Vector3 pos, float rotY, Transform padre,
        Material material, float ancho = 1.1f, float alto = 0.45f)
    {
        var placa = GameObject.CreatePrimitive(PrimitiveType.Cube);
        placa.name = $"Senal '{texto.Replace('\n', ' ')}'";
        placa.transform.SetParent(padre);
        placa.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, rotY, 0f));
        placa.transform.localScale = new Vector3(ancho, alto, 0.05f);
        placa.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(placa.GetComponent<Collider>());
        placa.AddComponent<SenalEvacuacion>();

        var textoGo = new GameObject("Texto");
        textoGo.transform.SetParent(placa.transform.parent);
        textoGo.transform.SetPositionAndRotation(
            pos + placa.transform.rotation * new Vector3(0f, 0f, 0.035f),
            Quaternion.Euler(0f, rotY + 180f, 0f)); // TMP mira hacia -z de su transform
        var tmp = textoGo.AddComponent<TextMeshPro>();
        tmp.text = texto;
        tmp.fontSize = 2.2f;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.rectTransform.sizeDelta = new Vector2(ancho * 0.95f, alto * 0.95f);
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 0.4f;
        tmp.fontSizeMax = 3f;
        // El texto queda como hermano de la placa (no hijo) para evitar
        // la deformación por la escala no uniforme del cubo.
        return placa;
    }

    // ------------------------------------------------------------------
    //  Mobiliario
    // ------------------------------------------------------------------
    private static void ConstruirMobiliario(Transform padre)
    {
        var g = Grupo("Mobiliario", padre);

        // (centro x, centro z, piso y, ¿pizarra al sur?)
        var aulas = new (string nombre, float x, float z, float y)[]
        {
            ("Aula 101", -10f, -4.85f, 0f),
            ("Aula 102", -2f, -4.85f, 0f),
            ("Laboratorio", 8f, -4.85f, 0f),
            ("Aula 103", -9f, 4.85f, 0f),
            ("Direccion", 0f, 4.85f, 0f),
            ("Aula 201", -10f, -4.85f, PisoDos),
            ("Aula 202", -2f, -4.85f, PisoDos),
            ("Biblioteca", 8f, -4.85f, PisoDos),
            ("Aula 203", -9f, 4.85f, PisoDos),
            ("Sala Profesores", 0f, 4.85f, PisoDos),
        };

        foreach (var aula in aulas)
        {
            var ga = Grupo(aula.nombre, g);

            // Pizarra en la pared más lejana al pasillo
            float zPizarra = aula.z < 0f ? -7.85f : 7.85f;
            float rotPizarra = aula.z < 0f ? 0f : 180f;
            var pizarra = Caja($"Pizarra {aula.nombre}",
                new Vector3(aula.x, aula.y + 1.6f, zPizarra),
                new Vector3(3f, 1.2f, 0.06f), matPizarra, ga);
            pizarra.transform.rotation = Quaternion.Euler(0f, rotPizarra, 0f);

            // Carpetas en cuadrícula 2x2
            foreach (float dx in new[] { -1.6f, 1.6f })
                foreach (float dz in new[] { -1.4f, 1.2f })
                    Escritorio(ga, new Vector3(aula.x + dx, aula.y, aula.z + dz));
        }
    }

    private static void Escritorio(Transform padre, Vector3 pos)
    {
        var g = Grupo($"Carpeta ({pos.x:F0},{pos.z:F0})", padre);
        Caja("Tablero", pos + new Vector3(0f, 0.74f, 0f), new Vector3(1.1f, 0.05f, 0.6f), matMadera, g);
        Caja("Pata Izq", pos + new Vector3(-0.5f, 0.36f, 0f), new Vector3(0.05f, 0.72f, 0.55f), matMetal, g);
        Caja("Pata Der", pos + new Vector3(0.5f, 0.36f, 0f), new Vector3(0.05f, 0.72f, 0.55f), matMetal, g);
        Caja("Silla", pos + new Vector3(0f, 0.23f, 0.55f), new Vector3(0.45f, 0.46f, 0.45f), matMadera, g);
    }

    // ------------------------------------------------------------------
    //  Peligros y decisiones
    // ------------------------------------------------------------------
    private static void ConstruirZonasPeligro(Transform padre)
    {
        var g = Grupo("Zonas de Peligro", padre);
        CrearFuego(g, "Fuego Pasillo Oeste P2", new Vector3(-13f, PisoDos, 0f), "el humo del pasillo oeste (piso 2)");
        CrearFuego(g, "Fuego Pasillo Este P1", new Vector3(12.5f, 0f, 0f), "el fuego del pasillo este (piso 1)");
        CrearFuego(g, "Fuego Laboratorio P1", new Vector3(11f, 0f, -6f), "el incendio del laboratorio");
    }

    private static void CrearFuego(Transform padre, string nombre, Vector3 pos, string nombreZona)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(padre);
        go.transform.position = pos;

        var zona = go.AddComponent<ZonaPeligro>();
        var so = new SerializedObject(zona);
        so.FindProperty("nombreZona").stringValue = nombreZona;
        so.FindProperty("radio").floatValue = 1.7f;
        so.ApplyModifiedPropertiesWithoutUndo();

        var matParticulas = AssetDatabase.LoadAssetAtPath<Material>(RutaMatParticulas);

        Particulas(go.transform, "Fuego", pos, matParticulas,
            new Color(1f, 0.55f, 0.1f, 0.9f), new Color(1f, 0.15f, 0.05f, 0.8f),
            velocidad: 1.8f, tam: 0.55f, vida: 0.9f, tasa: 45f, radioCono: 0.5f);
        Particulas(go.transform, "Humo", pos + Vector3.up * 0.8f, matParticulas,
            new Color(0.25f, 0.25f, 0.25f, 0.5f), new Color(0.1f, 0.1f, 0.1f, 0.35f),
            velocidad: 1f, tam: 1.1f, vida: 2.4f, tasa: 18f, radioCono: 0.7f);

        var luzGo = new GameObject("Luz Fuego");
        luzGo.transform.SetParent(go.transform);
        luzGo.transform.position = pos + Vector3.up * 0.9f;
        var luz = luzGo.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.color = new Color(1f, 0.5f, 0.15f);
        luz.intensity = 2.2f;
        luz.range = 7f;
        luz.shadows = LightShadows.None;
    }

    private static void Particulas(Transform padre, string nombre, Vector3 pos, Material mat,
        Color colorA, Color colorB, float velocidad, float tam, float vida, float tasa, float radioCono)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(padre);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(-90f, 0f, 0f); // cono hacia arriba

        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(colorA, colorB);
        main.startSize = new ParticleSystem.MinMaxCurve(tam * 0.6f, tam);
        main.startLifetime = new ParticleSystem.MinMaxCurve(vida * 0.7f, vida);
        main.startSpeed = new ParticleSystem.MinMaxCurve(velocidad * 0.7f, velocidad);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 300;
        main.playOnAwake = false;

        var emision = ps.emission;
        emision.rateOverTime = tasa;

        var forma = ps.shape;
        forma.shapeType = ParticleSystemShapeType.Cone;
        forma.angle = 12f;
        forma.radius = radioCono;

        // Las partículas se desvanecen al final de su vida.
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0f, 1f) });
        col.color = new ParticleSystem.MinMaxGradient(grad);

        if (mat != null)
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = mat;
    }

    private static void ConstruirPuntosDecision(Transform padre)
    {
        var g = Grupo("Puntos de Decision", padre);
        Decision(g, "bajar por la escalera de emergencia", true, new Vector3(12f, PisoDos, 4.5f), 1.6f);
        Decision(g, "avanzar por el pasillo hacia la escalera", true, new Vector3(4f, PisoDos, 0f), 1.8f);
        Decision(g, "acercarte al ascensor durante el incendio", false, new Vector3(5.5f, PisoDos, 5.4f), 1.3f);
        Decision(g, "ir hacia el humo del pasillo oeste", false, new Vector3(-11.5f, PisoDos, 0f), 1.5f);
        Decision(g, "dirigirte a la salida principal por el pasillo oeste", true, new Vector3(-6f, 0f, 0f), 1.8f);
        Decision(g, "acercarte al fuego del pasillo este", false, new Vector3(10.5f, 0f, 0f), 1.5f);
        Decision(g, "salir por la puerta principal", true, new Vector3(-14.5f, 0f, 0f), 1.6f);
    }

    private static void Decision(Transform padre, string descripcion, bool correcta, Vector3 pos, float radio)
    {
        var go = new GameObject($"Decision {(correcta ? "OK" : "MAL")}: {descripcion}");
        go.transform.SetParent(padre);
        go.transform.position = pos;
        var pd = go.AddComponent<PuntoDecision>();
        var so = new SerializedObject(pd);
        so.FindProperty("descripcion").stringValue = descripcion;
        so.FindProperty("esCorrecta").boolValue = correcta;
        so.FindProperty("radio").floatValue = radio;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ------------------------------------------------------------------
    //  Teleportación
    // ------------------------------------------------------------------
    private static void ConstruirTeleportacion(Transform padre)
    {
        var g = Grupo("Teleportacion", padre);
        var prefabArea = AssetDatabase.LoadAssetAtPath<GameObject>(RutaAreaTeleport);
        var prefabAncla = AssetDatabase.LoadAssetAtPath<GameObject>(RutaAnclaTeleport);
        if (prefabArea == null)
        {
            Debug.LogWarning("[ConstructorEscuela] No se encontró el prefab Teleport Area; revisa los samples de XRIT.");
            return;
        }

        // ---- Piso 1 ----
        Area(g, prefabArea, "Area Pasillo P1", new Vector3(0f, 0f, 0f), 27.6f, 3f);
        Area(g, prefabArea, "Area Aula 101", new Vector3(-10f, 0f, -4.85f), 7.6f, 6.2f);
        Area(g, prefabArea, "Area Aula 102", new Vector3(-2f, 0f, -4.85f), 7.6f, 6.2f);
        Area(g, prefabArea, "Area Laboratorio", new Vector3(8f, 0f, -4.85f), 11.6f, 6.2f);
        Area(g, prefabArea, "Area Aula 103", new Vector3(-9f, 0f, 4.85f), 9.6f, 6.2f);
        Area(g, prefabArea, "Area Direccion", new Vector3(0f, 0f, 4.85f), 7.6f, 6.2f);
        Area(g, prefabArea, "Area Hall Escalera P1", new Vector3(6.5f, 0f, 4.85f), 4.6f, 6.2f);
        Area(g, prefabArea, "Area Patio", new Vector3(-18.5f, 0f, 0f), 9f, 12f);

        // ---- Piso 2 ----
        Area(g, prefabArea, "Area Pasillo P2", new Vector3(0f, PisoDos, 0f), 27.6f, 3f);
        Area(g, prefabArea, "Area Aula 201", new Vector3(-10f, PisoDos, -4.85f), 7.6f, 6.2f);
        Area(g, prefabArea, "Area Aula 202", new Vector3(-2f, PisoDos, -4.85f), 7.6f, 6.2f);
        Area(g, prefabArea, "Area Biblioteca", new Vector3(8f, PisoDos, -4.85f), 11.6f, 6.2f);
        Area(g, prefabArea, "Area Aula 203", new Vector3(-9f, PisoDos, 4.85f), 9.6f, 6.2f);
        Area(g, prefabArea, "Area Sala Profesores", new Vector3(0f, PisoDos, 4.85f), 7.6f, 6.2f);
        Area(g, prefabArea, "Area Hall Escalera P2", new Vector3(6f, PisoDos, 4.85f), 3.6f, 6.2f);
        Area(g, prefabArea, "Area Descanso Escalera", new Vector3(13.45f, PisoDos, 4.5f), 1f, 3.9f);

        // ---- Anclas sobre la rampa de la escalera ----
        if (prefabAncla != null)
        {
            Ancla(g, prefabAncla, "Ancla Escalera Abajo", new Vector3(8.8f, 0.25f, 4.5f), -90f);
            Ancla(g, prefabAncla, "Ancla Escalera Medio", new Vector3(10.7f, 1.85f, 4.5f), -90f);
            Ancla(g, prefabAncla, "Ancla Escalera Arriba", new Vector3(12.6f, 3.32f, 4.5f), 90f);
        }
    }

    private static void Area(Transform padre, GameObject prefab, string nombre, Vector3 centro, float sx, float sz)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.name = nombre;
        go.transform.SetParent(padre);
        go.transform.position = centro + Vector3.up * 0.005f;
        // El prefab tiene un cubo hijo con escala (10, 0.25, 5): se compensa
        // para que la placa final mida exactamente sx × sz y 2 cm de grosor.
        go.transform.localScale = new Vector3(sx / 10f, 0.08f, sz / 5f);
    }

    private static void Ancla(Transform padre, GameObject prefab, string nombre, Vector3 pos, float rotY)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.name = nombre;
        go.transform.SetParent(padre);
        go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, rotY, 0f));
    }

    // ------------------------------------------------------------------
    //  Rig XR, HUD y gestores
    // ------------------------------------------------------------------
    private static GameObject ColocarRigXR(Transform puntoInicio)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RutaRig);
        if (prefab == null)
            return null;

        var rig = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        rig.name = "XR Origin (XR Rig)";
        rig.transform.SetPositionAndRotation(puntoInicio.position, puntoInicio.rotation);
        return rig;
    }

    private static GameObject ConstruirHUD()
    {
        var canvasGo = new GameObject("HUD Evacuacion");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        var rt = canvasGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(900f, 520f);
        canvasGo.transform.localScale = Vector3.one * 0.0022f;
        canvasGo.transform.position = new Vector3(-11.5f, PisoDos + 1.6f, -4f);

        var estado = TextoHUD(canvasGo.transform, "TextoEstado", new Vector2(0f, 190f), new Vector2(860f, 80f), 54f, FontStyles.Bold);
        var crono = TextoHUD(canvasGo.transform, "TextoCronometro", new Vector2(0f, 120f), new Vector2(860f, 60f), 44f, FontStyles.Normal);
        var reporte = TextoHUD(canvasGo.transform, "TextoReporte", new Vector2(0f, -30f), new Vector2(860f, 230f), 32f, FontStyles.Normal);

        // Barra de subtítulos accesible (fondo + texto)
        var fondoGo = new GameObject("FondoSubtitulo");
        fondoGo.transform.SetParent(canvasGo.transform, false);
        var fondo = fondoGo.AddComponent<Image>();
        fondo.color = new Color(0f, 0f, 0f, 0.6f);
        var fondoRt = fondoGo.GetComponent<RectTransform>();
        fondoRt.anchoredPosition = new Vector2(0f, -205f);
        fondoRt.sizeDelta = new Vector2(880f, 110f);
        fondo.enabled = false;

        var subtitulo = TextoHUD(fondoGo.transform, "TextoSubtitulo", Vector2.zero, new Vector2(860f, 100f), 34f, FontStyles.Normal);

        var hud = canvasGo.AddComponent<HUDEvacuacion>();
        var soHud = new SerializedObject(hud);
        soHud.FindProperty("textoEstado").objectReferenceValue = estado;
        soHud.FindProperty("textoCronometro").objectReferenceValue = crono;
        soHud.FindProperty("textoReporte").objectReferenceValue = reporte;
        soHud.ApplyModifiedPropertiesWithoutUndo();

        // Guarda referencias para conectar el AccesibilidadManager después.
        canvasGo.name = "HUD Evacuacion";
        hudSubtitulo = subtitulo;
        hudFondoSubtitulo = fondo;
        return canvasGo;
    }

    private static TextMeshProUGUI hudSubtitulo;
    private static Image hudFondoSubtitulo;

    private static TextMeshProUGUI TextoHUD(Transform padre, string nombre, Vector2 pos, Vector2 tam, float fuente, FontStyles estilo)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.rectTransform.anchoredPosition = pos;
        tmp.rectTransform.sizeDelta = tam;
        tmp.fontSize = fuente;
        tmp.fontStyle = estilo;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.text = "";
        return tmp;
    }

    private static void ConstruirGestores(Transform padre, Transform puntoInicio, GameObject hud)
    {
        var g = new GameObject("== GESTORES ==");
        g.transform.SetParent(padre);

        var gestor = g.AddComponent<GestorEvacuacion>();
        g.AddComponent<EvaluadorDecisiones>();
        var accesibilidad = g.AddComponent<AccesibilidadManager>();
        var teleport = g.AddComponent<TeleportSystem>();
        g.AddComponent<BenchmarkXR>();

        // Alarma con luces de emergencia en ambos pisos
        var alarmaGo = new GameObject("Alarma de Incendio");
        alarmaGo.transform.SetParent(g.transform);
        alarmaGo.transform.position = new Vector3(0f, AltoPiso - 0.3f, 0f);
        var alarma = alarmaGo.AddComponent<AlarmaIncendio>();
        LuzEmergencia(alarmaGo.transform, new Vector3(-6f, AltoPiso - 0.4f, 0f));
        LuzEmergencia(alarmaGo.transform, new Vector3(6f, AltoPiso - 0.4f, 0f));
        LuzEmergencia(alarmaGo.transform, new Vector3(-6f, PisoDos + AltoPiso - 0.4f, 0f));
        LuzEmergencia(alarmaGo.transform, new Vector3(6f, PisoDos + AltoPiso - 0.4f, 0f));

        // Cableado de referencias serializadas
        var soGestor = new SerializedObject(gestor);
        soGestor.FindProperty("alarma").objectReferenceValue = alarma;
        soGestor.ApplyModifiedPropertiesWithoutUndo();

        var soTeleport = new SerializedObject(teleport);
        soTeleport.FindProperty("puntoInicio").objectReferenceValue = puntoInicio;
        soTeleport.ApplyModifiedPropertiesWithoutUndo();

        var soAcc = new SerializedObject(accesibilidad);
        soAcc.FindProperty("textoSubtitulo").objectReferenceValue = hudSubtitulo;
        soAcc.FindProperty("fondoSubtitulo").objectReferenceValue = hudFondoSubtitulo;
        soAcc.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void LuzEmergencia(Transform padre, Vector3 pos)
    {
        var go = new GameObject($"Luz Emergencia {pos}");
        go.transform.SetParent(padre);
        go.transform.position = pos;
        var l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = Color.red;
        l.intensity = 2.5f;
        l.range = 10f;
        l.shadows = LightShadows.None;
        l.enabled = false;
    }

    // ------------------------------------------------------------------
    //  Utilidades de construcción
    // ------------------------------------------------------------------
    private static Transform Grupo(string nombre, Transform padre)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(padre);
        return go.transform;
    }

    private static GameObject Caja(string nombre, Vector3 centro, Vector3 tam, Material material, Transform padre)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = nombre;
        go.transform.SetParent(padre);
        go.transform.position = centro;
        go.transform.localScale = tam;
        go.GetComponent<Renderer>().sharedMaterial = material;
        return go;
    }

    /// <summary>Pared a lo largo del eje X en z fija, con puerta opcional centrada en xPuerta.</summary>
    private static void ParedX(Transform padre, Material mat, float z, float x0, float x1,
        float y0, float alto, float? xPuerta = null, float anchoPuerta = 1.2f, float altoPuerta = 2.2f)
    {
        if (!xPuerta.HasValue)
        {
            Caja($"Pared z{z} [{x0},{x1}]", new Vector3((x0 + x1) / 2f, y0 + alto / 2f, z),
                new Vector3(x1 - x0, alto, 0.2f), mat, padre);
            return;
        }

        float p = xPuerta.Value, mitad = anchoPuerta / 2f;
        if (p - mitad > x0)
            Caja($"Pared z{z} [{x0},{p - mitad}]", new Vector3((x0 + p - mitad) / 2f, y0 + alto / 2f, z),
                new Vector3(p - mitad - x0, alto, 0.2f), mat, padre);
        if (x1 > p + mitad)
            Caja($"Pared z{z} [{p + mitad},{x1}]", new Vector3((p + mitad + x1) / 2f, y0 + alto / 2f, z),
                new Vector3(x1 - (p + mitad), alto, 0.2f), mat, padre);
        // Dintel sobre la puerta
        Caja($"Dintel z{z} x{p}", new Vector3(p, y0 + altoPuerta + (alto - altoPuerta) / 2f, z),
            new Vector3(anchoPuerta, alto - altoPuerta, 0.2f), mat, padre);
    }

    /// <summary>Pared a lo largo del eje Z en x fija, con puerta opcional centrada en zPuerta.</summary>
    private static void ParedZ(Transform padre, Material mat, float x, float z0, float z1,
        float y0, float alto, float? zPuerta = null, float anchoPuerta = 1.2f, float altoPuerta = 2.2f)
    {
        if (!zPuerta.HasValue)
        {
            Caja($"Pared x{x} [{z0},{z1}]", new Vector3(x, y0 + alto / 2f, (z0 + z1) / 2f),
                new Vector3(0.2f, alto, z1 - z0), mat, padre);
            return;
        }

        float p = zPuerta.Value, mitad = anchoPuerta / 2f;
        if (p - mitad > z0)
            Caja($"Pared x{x} [{z0},{p - mitad}]", new Vector3(x, y0 + alto / 2f, (z0 + p - mitad) / 2f),
                new Vector3(0.2f, alto, p - mitad - z0), mat, padre);
        if (z1 > p + mitad)
            Caja($"Pared x{x} [{p + mitad},{z1}]", new Vector3(x, y0 + alto / 2f, (p + mitad + z1) / 2f),
                new Vector3(0.2f, alto, z1 - (p + mitad)), mat, padre);
        Caja($"Dintel x{x} z{p}", new Vector3(x, y0 + altoPuerta + (alto - altoPuerta) / 2f, p),
            new Vector3(0.2f, alto - altoPuerta, anchoPuerta), mat, padre);
    }

    private static void AgregarABuildSettings()
    {
        var escenas = new System.Collections.Generic.List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(RutaEscena, true)
        };
        foreach (var e in EditorBuildSettings.scenes)
            if (e.path != RutaEscena)
                escenas.Add(e);
        EditorBuildSettings.scenes = escenas.ToArray();
    }
}
