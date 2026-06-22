# Arquitectura Técnica — VR Evacuation Trainer Inclusivo

## Visión general del sistema

```
Capa de presentación (XR)
├── Unity 2022.3.62f1
├── Google Cardboard XR Plugin + XR Interaction Toolkit
└── UI/UX en Unity Canvas (World Space)

Capa de lógica de negocio (Scripts C#)
├── Gestores de AR (ARPlaneManager, ARTrackedImageManager, etc.)
├── Controllers de interacción
└── Servicios de datos (UnityWebRequest)

Capa de datos (si aplica)
├── API REST (JSON)
├── Local Storage (PlayerPrefs / JSON files)
└── Assets de Unity (Prefabs, Modelos 3D)
```

## Componentes principales

| Componente | Propósito | Script asociado |
|-----------|---------|----------------|
| | | |
| | | |
| | | |

## Diagrama de escena

```
Hierarchy:
├── AR Session
├── XR Origin
│   └── Camera Offset
│       └── Main Camera
├── [Componentes específicos del proyecto]
└── Canvas UI
```

## Decisiones de diseño

| Decisión | Alternativa descartada | Por qué se eligió |
|---------|----------------------|------------------|
| | | |

## Métricas de rendimiento

| Métrica | Objetivo | Obtenido |
|---------|---------|---------|
| FPS promedio | ≥30 (AR) | |
| Draw Calls | ≤100 | |
| SUS Score | ≥65 | |
| Tiempo de inicio | ≤5s | |

*Actualizar con datos reales al finalizar el proyecto*
