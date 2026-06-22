# Guía de Contribución — VR Evacuation Trainer Inclusivo

## Flujo de trabajo del equipo

### Ramas

| Rama | Propósito |
|------|-----------|
| `main` | Versión estable y entregable |
| `desarrollo` | Integración de features |
| `feature/nombre-feature` | Desarrollo de una función específica |
| `fix/nombre-bug` | Corrección de bugs |

### Workflow para cada semana

```bash
# 1. Actualizar tu rama desde main
git checkout desarrollo
git pull origin desarrollo

# 2. Crear rama para tu feature
git checkout -b feature/S09-escenario-3d

# 3. Hacer cambios y commits
git add Assets/Scenes/EscenaPrincipal.unity
git commit -m "S09: agregar escenario 3D del laboratorio con ProBuilder"

# 4. Subir y crear PR
git push origin feature/S09-escenario-3d
# En GitHub: crear PR de feature → desarrollo
```

### Commits — Formato estándar

```
SXXX: descripción corta (qué se hizo)

Ej:
S09: implementar ARPlaneManager en escena principal
S10: agregar UI flotante de información del objeto
fix: corregir crash al perder tracking del marcador
```

### Code Review

- Toda PR debe ser revisada por al menos 1 compañero antes de mergear
- El PR debe incluir capturas o video del cambio

## Estándares de código C#

```csharp
// Nombres de clase: PascalCase
public class GestorMarcadores : MonoBehaviour

// Nombres de método: PascalCase
void ActualizarUI(ARTrackedImage imagen)

// Nombres de variable: camelCase
private float tiempoMirando;
public TextMeshPro textoEstado;

// Campos públicos en Inspector: nombres descriptivos
[Header("Configuración")]
public float velocidadRotacion = 0.3f;
```
