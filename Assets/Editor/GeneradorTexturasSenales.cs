using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Genera por código los pictogramas de las señales de seguridad peruanas
/// (NTP 399.010-1) como PNG en Assets/Senales/: hombre corriendo con flecha,
/// S de zona segura en caso de sismos, extintor, punto de reunión y
/// prohibido usar ascensor.
///
/// IMPORTANTE: si ya existe un PNG con el mismo nombre (por ejemplo, la
/// señal oficial de INDECI descargada de internet), NO se sobrescribe:
/// el proyecto usa automáticamente la imagen real que coloques en la carpeta.
/// </summary>
public static class GeneradorTexturasSenales
{
    public const string Carpeta = "Assets/Senales";

    private static readonly Color VerdeSeguridad = new Color(0.00f, 0.42f, 0.19f);
    private static readonly Color RojoSeguridad = new Color(0.78f, 0.05f, 0.04f);
    private static readonly Color Blanco = Color.white;
    private static readonly Color Negro = new Color(0.05f, 0.05f, 0.05f);

    private static Color32[] px;
    private static int W, H;
    private static bool espejo;

    [MenuItem("Herramientas/Evacuation Trainer/Regenerar Pictogramas de Senales")]
    public static void GenerarTodas()
    {
        if (!AssetDatabase.IsValidFolder(Carpeta))
            AssetDatabase.CreateFolder("Assets", "Senales");

        Generar("ruta_evacuacion_der.png", 512, 256, VerdeSeguridad, () => RutaEvacuacion(false));
        Generar("ruta_evacuacion_izq.png", 512, 256, VerdeSeguridad, () => RutaEvacuacion(true));
        Generar("zona_segura_sismo.png", 256, 256, Blanco, ZonaSegura);
        Generar("extintor.png", 256, 256, RojoSeguridad, Extintor);
        Generar("punto_reunion.png", 256, 256, VerdeSeguridad, PuntoReunion);
        Generar("no_ascensor.png", 256, 256, Blanco, NoAscensor);

        AssetDatabase.Refresh();
        Debug.Log($"[GeneradorTexturasSenales] Pictogramas listos en {Carpeta}. " +
                  "Para usar las señales oficiales de INDECI, reemplaza los PNG manteniendo el nombre.");
    }

    private static void Generar(string nombre, int w, int h, Color fondo, System.Action dibujar)
    {
        string ruta = $"{Carpeta}/{nombre}";
        if (File.Exists(ruta))
            return; // respeta la señal oficial colocada por el usuario

        W = w; H = h; espejo = false;
        px = new Color32[w * h];
        var f = (Color32)fondo;
        for (int i = 0; i < px.Length; i++) px[i] = f;

        dibujar();

        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.SetPixels32(px);
        tex.Apply();
        File.WriteAllBytes(ruta, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    // ------------------------------------------------------------------
    //  Pictogramas
    // ------------------------------------------------------------------

    /// <summary>Hombre corriendo hacia la flecha (base de toda señal de ruta).</summary>
    private static void RutaEvacuacion(bool haciaIzquierda)
    {
        espejo = haciaIzquierda;
        Corredor(0.08f, Blanco);

        // Flecha
        Linea(0.58f, 0.5f, 0.78f, 0.5f, 0.045f, Blanco);
        Triangulo(0.76f, 0.66f, 0.76f, 0.34f, 0.92f, 0.5f, Blanco);
        espejo = false;
    }

    /// <summary>Figura humana corriendo, dibujada con círculo y trazos gruesos.</summary>
    private static void Corredor(float xBase, Color c)
    {
        float g = 0.045f; // grosor de trazo (fracción de H)
        Circulo(xBase + 0.24f, 0.80f, 0.075f, c);                          // cabeza
        Linea(xBase + 0.22f, 0.70f, xBase + 0.30f, 0.46f, g, c);           // torso inclinado
        Linea(xBase + 0.24f, 0.63f, xBase + 0.38f, 0.56f, g, c);           // brazo adelante
        Linea(xBase + 0.24f, 0.63f, xBase + 0.12f, 0.52f, g, c);           // brazo atrás
        Linea(xBase + 0.30f, 0.46f, xBase + 0.42f, 0.36f, g, c);           // muslo adelante
        Linea(xBase + 0.42f, 0.36f, xBase + 0.40f, 0.16f, g, c);           // pierna adelante
        Linea(xBase + 0.30f, 0.46f, xBase + 0.19f, 0.32f, g, c);           // muslo atrás
        Linea(xBase + 0.19f, 0.32f, xBase + 0.06f, 0.26f, g, c);           // pierna atrás
    }

    /// <summary>Círculo verde con "S": zona segura en caso de sismos.</summary>
    private static void ZonaSegura()
    {
        Anillo(0.5f, 0.5f, 0.42f, 0.48f, 0f, 360f, VerdeSeguridad);
        // La "S" formada por dos arcos: el superior abre hacia abajo-derecha
        // y el inferior hacia arriba-izquierda.
        Anillo(0.5f, 0.615f, 0.085f, 0.155f, 0f, 270f, VerdeSeguridad);
        Anillo(0.5f, 0.385f, 0.085f, 0.155f, 180f, 450f, VerdeSeguridad);
    }

    /// <summary>Silueta blanca de extintor sobre fondo rojo.</summary>
    private static void Extintor()
    {
        Rect(0.40f, 0.18f, 0.60f, 0.62f, Blanco);                          // cuerpo
        Circulo(0.50f, 0.62f, 0.10f, Blanco);                              // hombro redondeado
        Rect(0.46f, 0.70f, 0.54f, 0.76f, Blanco);                          // cuello
        Linea(0.50f, 0.76f, 0.62f, 0.80f, 0.03f, Blanco);                  // manija
        Linea(0.50f, 0.74f, 0.34f, 0.68f, 0.025f, Blanco);                 // manguera
        Linea(0.34f, 0.68f, 0.32f, 0.50f, 0.025f, Blanco);
        Rect(0.36f, 0.14f, 0.64f, 0.18f, Blanco);                          // base
    }

    /// <summary>Cuatro flechas hacia el centro con personas: punto de reunión.</summary>
    private static void PuntoReunion()
    {
        // Flechas desde las esquinas hacia el centro
        foreach (var (x0, y0) in new[] { (0.12f, 0.12f), (0.88f, 0.12f), (0.12f, 0.88f), (0.88f, 0.88f) })
        {
            float dx = 0.5f - x0, dy = 0.5f - y0;
            float fx = x0 + dx * 0.42f, fy = y0 + dy * 0.42f;              // fin del asta
            Linea(x0, y0, fx, fy, 0.035f, Blanco);
            // Punta: triángulo apuntando al centro
            float px1 = fx + dx * 0.22f, py1 = fy + dy * 0.22f;
            float nx = -dy * 0.16f, ny = dx * 0.16f;                       // perpendicular
            Triangulo(fx + nx, fy + ny, fx - nx, fy - ny, px1, py1, Blanco);
        }
        // Tres personas al centro
        foreach (float cx in new[] { 0.40f, 0.50f, 0.60f })
        {
            Circulo(cx, 0.56f, 0.045f, Blanco);
            Rect(cx - 0.04f, 0.38f, cx + 0.04f, 0.50f, Blanco);
        }
    }

    /// <summary>Ascensor tachado en rojo: no usar en emergencias.</summary>
    private static void NoAscensor()
    {
        // Cabina con dos personas
        RectBorde(0.28f, 0.25f, 0.72f, 0.75f, 0.03f, Negro);
        foreach (float cx in new[] { 0.42f, 0.58f })
        {
            Circulo(cx, 0.60f, 0.045f, Negro);
            Rect(cx - 0.045f, 0.36f, cx + 0.045f, 0.53f, Negro);
        }
        // Prohibición
        Anillo(0.5f, 0.5f, 0.40f, 0.47f, 0f, 360f, RojoSeguridad);
        Linea(0.22f, 0.78f, 0.78f, 0.22f, 0.045f, RojoSeguridad);
    }

    // ------------------------------------------------------------------
    //  Rasterizador (coordenadas normalizadas 0..1)
    // ------------------------------------------------------------------

    private static void Pixel(int x, int y, Color32 c)
    {
        if (espejo) x = W - 1 - x;
        if (x >= 0 && x < W && y >= 0 && y < H)
            px[y * W + x] = c;
    }

    private static void Rect(float x0, float y0, float x1, float y1, Color c)
    {
        var c32 = (Color32)c;
        for (int y = (int)(y0 * H); y <= (int)(y1 * H); y++)
            for (int x = (int)(x0 * W); x <= (int)(x1 * W); x++)
                Pixel(x, y, c32);
    }

    private static void RectBorde(float x0, float y0, float x1, float y1, float grosor, Color c)
    {
        Rect(x0, y0, x1, y0 + grosor, c);
        Rect(x0, y1 - grosor, x1, y1, c);
        Rect(x0, y0, x0 + grosor, y1, c);
        Rect(x1 - grosor, y0, x1, y1, c);
    }

    private static void Circulo(float cx, float cy, float r, Color c)
    {
        var c32 = (Color32)c;
        float rp = r * H;
        int pcx = (int)(cx * W), pcy = (int)(cy * H);
        for (int y = (int)(pcy - rp); y <= pcy + rp; y++)
            for (int x = (int)(pcx - rp); x <= pcx + rp; x++)
                if ((x - pcx) * (x - pcx) + (y - pcy) * (y - pcy) <= rp * rp)
                    Pixel(x, y, c32);
    }

    /// <summary>Anillo entre dos radios, limitado a un rango angular en grados (0° = este, antihorario).</summary>
    private static void Anillo(float cx, float cy, float rIn, float rOut, float a0, float a1, Color c)
    {
        var c32 = (Color32)c;
        float rInP = rIn * H, rOutP = rOut * H;
        int pcx = (int)(cx * W), pcy = (int)(cy * H);
        for (int y = (int)(pcy - rOutP); y <= pcy + rOutP; y++)
            for (int x = (int)(pcx - rOutP); x <= pcx + rOutP; x++)
            {
                float dx = x - pcx, dy = y - pcy;
                float d2 = dx * dx + dy * dy;
                if (d2 < rInP * rInP || d2 > rOutP * rOutP)
                    continue;
                float ang = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                if (ang < a0) ang += 360f;
                if (ang >= a0 && ang <= a1)
                    Pixel(x, y, c32);
            }
    }

    private static void Linea(float x0, float y0, float x1, float y1, float grosor, Color c)
    {
        var c32 = (Color32)c;
        Vector2 a = new Vector2(x0 * W, y0 * H), b = new Vector2(x1 * W, y1 * H);
        float g = grosor * H * 0.5f;
        int minX = (int)Mathf.Min(a.x, b.x) - (int)g - 1, maxX = (int)Mathf.Max(a.x, b.x) + (int)g + 1;
        int minY = (int)Mathf.Min(a.y, b.y) - (int)g - 1, maxY = (int)Mathf.Max(a.y, b.y) + (int)g + 1;
        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                var p = new Vector2(x, y);
                float t = Mathf.Clamp01(Vector2.Dot(p - a, b - a) / Mathf.Max(0.0001f, (b - a).sqrMagnitude));
                if (Vector2.Distance(p, a + t * (b - a)) <= g)
                    Pixel(x, y, c32);
            }
    }

    private static void Triangulo(float x0, float y0, float x1, float y1, float x2, float y2, Color c)
    {
        var c32 = (Color32)c;
        Vector2 a = new Vector2(x0 * W, y0 * H), b = new Vector2(x1 * W, y1 * H), d = new Vector2(x2 * W, y2 * H);
        int minX = (int)Mathf.Min(a.x, Mathf.Min(b.x, d.x)), maxX = (int)Mathf.Max(a.x, Mathf.Max(b.x, d.x));
        int minY = (int)Mathf.Min(a.y, Mathf.Min(b.y, d.y)), maxY = (int)Mathf.Max(a.y, Mathf.Max(b.y, d.y));
        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                var p = new Vector2(x, y);
                float s1 = Cruz(a, b, p), s2 = Cruz(b, d, p), s3 = Cruz(d, a, p);
                bool neg = s1 < 0 || s2 < 0 || s3 < 0;
                bool pos = s1 > 0 || s2 > 0 || s3 > 0;
                if (!(neg && pos))
                    Pixel(x, y, c32);
            }
    }

    private static float Cruz(Vector2 a, Vector2 b, Vector2 p)
        => (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x);
}
