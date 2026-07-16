# Evacuation Trainer Inclusivo (VR)

**Grupo G01 · PSISP08075 Realidad Virtual y Aumentada · Universidad Autónoma del Perú · 2026-1**

Simulador VR de evacuación ante incendios en una **escuela de 2 pisos**, diseñado con foco en
**accesibilidad** (subtítulos de todos los eventos sonoros, cues de audio, modo alto contraste y
locomoción por teleportación anti-mareo). El usuario inicia una clase normal en el Aula 201
(segundo piso); al sonar la alarma debe evacuar tomando decisiones reales (escalera vs. ascensor,
ruta despejada vs. pasillo con humo) mientras el sistema evalúa cada decisión y entrega una
nota final sobre 20 puntos.

## Integrantes y roles

| Integrante | Rol | Scripts / módulos a cargo |
|------------|-----|---------------------------|
| _(completar)_ | Líder técnico / flujo del simulacro | `GestorEvacuacion.cs`, `SalidaSegura.cs` |
| _(completar)_ | Locomoción VR | `TeleportSystem.cs`, anclas y áreas de teleport |
| _(completar)_ | Gameplay y evaluación | `EvaluadorDecisiones.cs`, `PuntoDecision.cs`, `ZonaPeligro.cs` |
| _(completar)_ | Accesibilidad y UI | `AccesibilidadManager.cs`, `HUDEvacuacion.cs`, `SenalEvacuacion.cs` |
| _(completar)_ | Escenario, audio y pruebas | `ConstructorEscuela.cs`, `AlarmaIncendio.cs`, `BenchmarkXR.cs`, plan de pruebas |

## Stack tecnológico

- **Unity 6000.5.3f1** (URP 17.5)
- **XR Interaction Toolkit 3.4.1** (teleportación, interactors, viñeta de confort)
- **OpenXR 1.17.1** (compatible con SteamVR / Quest Link / cualquier runtime OpenXR)
- **TextMeshPro** para señalización 3D y HUD
- **Input System** (nuevo) para atajos de demo

## Requisitos de instalación

1. Unity Hub con **Unity 6000.5.3f1** instalado (módulo *Windows Build Support* si se compila).
2. Un visor compatible con **OpenXR** (Quest 2/3 vía Link, o SteamVR). Sin visor, se puede usar el
   **XR Device Simulator** de XRIT para probar en escritorio.
3. Git.

## Instrucciones paso a paso (probadas)

```bash
git clone <URL-del-repo>
```

1. Abrir el proyecto con Unity Hub (carpeta raíz clonada). Unity restaura `Library/` automáticamente
   (la primera importación tarda varios minutos).
2. Si la escena aún no existe, ejecutar el menú:
   **Herramientas → Evacuation Trainer → Construir Escena Escuela (2 pisos)**.
   Esto genera `Assets/Scenes/EscuelaEvacuacion.unity` con todo cableado (edificio, fuego,
   señales, teleport, XR Origin, HUD y gestores).
3. Abrir `Assets/Scenes/EscuelaEvacuacion.unity` y pulsar **Play** con el visor conectado.
4. Flujo: apareces en el **Aula 201 (piso 2)** → a los 12 s suena la **alarma** → evacúa con
   teleport (gatillo) siguiendo las **señales verdes**: pasillo → escalera de emergencia →
   pasillo del piso 1 → puerta principal → **punto de encuentro** en el patio → se muestra tu
   **evaluación final**.

### Controles

| Acción | Control VR | Teclado (demo/pruebas) |
|--------|-----------|------------------------|
| Teletransportarse | Empujar stick / gatillo sobre área azul | XR Device Simulator |
| Reiniciar simulacro | — | `R` |
| Alto contraste | — | `C` |
| Tamaño de subtítulos | — | `+` / `-` |
| Benchmark FPS (60 s) | automático al sonar la alarma | `B` |

## Arquitectura

Diagrama completo y tabla de scripts en [`docs/arquitectura.md`](docs/arquitectura.md).

```
EscuelaEvacuacion.unity
├── == EDIFICIO ESCUELA ==      (piso 1, piso 2, escalera, ascensor, señales, mobiliario)
├── == JUGABILIDAD ==
│   ├── Zonas de Peligro        (3 focos de fuego/humo con ZonaPeligro)
│   ├── Puntos de Decision      (7 checkpoints correctos/incorrectos)
│   ├── Teleportacion           (16 áreas + 3 anclas en la escalera)
│   ├── PuntoInicio
│   └── == GESTORES ==          (GestorEvacuacion, EvaluadorDecisiones,
│                                AccesibilidadManager, TeleportSystem,
│                                BenchmarkXR, Alarma de Incendio)
├── XR Origin (XR Rig)          (XRIT Starter Assets: locomoción + interactors)
└── HUD Evacuacion              (canvas world-space que sigue a la cámara)
```

## Accesibilidad (lo "Inclusivo")

- **Subtítulos** de todos los eventos sonoros con fondo de alto contraste y tamaño ajustable.
- **Cues de audio** diferenciados: triple beep agudo = alerta, beep suave = información,
  clic corto = confirmación de teleport (usuarios con baja visión).
- **Modo alto contraste**: texto amarillo sobre negro y señales de evacuación con el doble de brillo.
- **Confort VR**: locomoción exclusivamente por teleportación + viñeta de túnel activable
  (`TeleportSystem.SetModoComodidad`), reduciendo el cybersickness.

## Capturas

_(agregar mínimo 3 screenshots reales en `docs/capturas/` y referenciarlas aquí)_

| Aula 201 (inicio) | Evacuación con fuego | Reporte final |
|---|---|---|
| ![inicio](docs/capturas/01_inicio.png) | ![fuego](docs/capturas/02_fuego.png) | ![reporte](docs/capturas/03_reporte.png) |

## Video demo

_(agregar link de YouTube no listado o Google Drive)_

## Resultados de pruebas

- Plan de pruebas: [`PLAN_PRUEBAS.md`](PLAN_PRUEBAS.md)
- Bugs documentados y cerrados: [`docs/BUGS.md`](docs/BUGS.md)
- **FPS promedio medido:** _(ejecutar `BenchmarkXR` — tecla `B` o automático con la alarma — y
  copiar aquí el resultado de `benchmark_xr.txt`)_. Objetivo VR: ≥ 72 FPS.
