# Evacuation Trainer Inclusivo (VR)

**Grupo G01 · PSISP08075 Realidad Virtual y Aumentada · Universidad Autónoma del Perú · 2026-1**

Simulador de **Realidad Virtual** para el entrenamiento de evacuación ante incendios en una
**escuela de 2 pisos**, diseñado con un foco explícito en la **accesibilidad e inclusión**
(subtítulos de todos los eventos sonoros, cues de audio diferenciados, modo de alto contraste,
ruta accesible para sillas de ruedas y locomoción por teleportación anti-mareo).

El usuario comienza una clase normal en el **Aula 201 (segundo piso)**; al sonar la alarma debe
evacuar tomando decisiones reales (escalera vs. ascensor, ruta despejada vs. pasillo con humo,
rampa accesible como alternativa) mientras el sistema **evalúa cada decisión en tiempo real** y
entrega una **nota final sobre 20 puntos** con el detalle de aciertos y errores.

---

## Tabla de contenido

- [Objetivo del proyecto](#objetivo-del-proyecto)
- [Características principales](#características-principales)
- [Integrantes y roles](#integrantes-y-roles)
- [Stack tecnológico](#stack-tecnológico)
- [Requisitos de instalación](#requisitos-de-instalación)
- [Instrucciones paso a paso](#instrucciones-paso-a-paso-probadas)
- [Controles](#controles)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Arquitectura](#arquitectura)
- [Accesibilidad (lo "Inclusivo")](#accesibilidad-lo-inclusivo)
- [Capturas](#capturas)
- [Video demo](#video-demo)
- [Resultados de pruebas](#resultados-de-pruebas)

---

## Objetivo del proyecto

Entrenar a estudiantes y personal de una institución educativa en el **protocolo correcto de
evacuación** ante un incendio, dentro de un entorno seguro y repetible de Realidad Virtual, y
hacerlo de forma **inclusiva** para que personas con discapacidad visual, auditiva o de movilidad
puedan usar y beneficiarse del simulador. El sistema mide objetivamente la calidad de la
evacuación para reforzar el aprendizaje.

---

## Características principales

- 🏫 **Escuela completa de 2 pisos** generada por código: aulas, pasillos, laboratorio, dirección,
  biblioteca, escalera de emergencia, rampa accesible, ascensor, mobiliario e iluminación.
- 🔥 **Incendio dinámico**: 3 focos de fuego/humo que se activan con la alarma y penalizan al
  usuario por proximidad.
- 🧭 **7 puntos de decisión** (4 correctos, 3 incorrectos) que registran cada elección del usuario.
- 📊 **Evaluación automática sobre 20 puntos** con reporte final detallado en el HUD.
- ♿ **Accesibilidad integral**: subtítulos, cues de audio, alto contraste, ruta accesible y
  confort VR (ver [sección dedicada](#accesibilidad-lo-inclusivo)).
- 🔊 **Audio 100% procedural** (`AudioClip.Create`): sirena y cues generados por código, sin
  dependencias de assets externos.
- 🖥️ **Modo escritorio** integrado: se puede probar sin visor con teclado y ratón.
- ⚙️ **Escena reproducible con un clic** desde un menú del Editor, ideal para trabajo en equipo.
- 📈 **Benchmark de rendimiento** integrado (FPS promedio, mínimo, máximo y 1% low).

---

## Integrantes y roles

| Integrante | Rol | Scripts / módulos a cargo |
|------------|-----|---------------------------|
| Juan Carlos Chacon | Líder técnico / flujo del simulacro | `GestorEvacuacion.cs`, `SalidaSegura.cs` |
| Juan Carlos Chacon  | Locomoción VR | `TeleportSystem.cs`, anclas y áreas de teleport |
| Juan Carlos Chacon  | Gameplay y evaluación | `EvaluadorDecisiones.cs`, `PuntoDecision.cs`, `ZonaPeligro.cs` |
| Juan Carlos Chacon  | Accesibilidad y UI | `AccesibilidadManager.cs`, `HUDEvacuacion.cs`, `SenalEvacuacion.cs` |
| Juan Carlos Chacon | Escenario, audio y pruebas | `ConstructorEscuela.cs`, `AlarmaIncendio.cs`, `BenchmarkXR.cs`, plan de pruebas |

---

## Stack tecnológico

| Capa | Tecnología |
|------|-----------|
| Motor | **Unity 6000.5.3f1** + URP 17.5 |
| XR | **OpenXR 1.17.1** + **XR Interaction Toolkit 3.4.1** (Starter Assets) |
| UI | **TextMeshPro** (señalización 3D + HUD world-space) |
| Input | **Input System** 1.19 (acciones XRI + atajos de teclado para demo) |
| Audio | Generado 100% por código (`AudioClip.Create`): sirena y cues de accesibilidad |

Compatible con cualquier runtime **OpenXR**: SteamVR, Meta Quest Link (Quest 2/3) o el
XR Device Simulator para pruebas en escritorio.

---

## Requisitos de instalación

1. **Unity Hub** con **Unity 6000.5.3f1** instalado (añadir el módulo *Windows Build Support (IL2CPP)*
   si se desea compilar un ejecutable).
2. Un visor compatible con **OpenXR** (Meta Quest 2/3 vía Link, o cualquier casco con SteamVR).
   Sin visor, se puede usar el **XR Device Simulator** de XRIT o el **modo escritorio** integrado.
3. **Git** para clonar el repositorio.
4. ~5 GB de espacio libre (el proyecto + la carpeta `Library/` que Unity regenera).

---

## Instrucciones paso a paso (probadas)

```bash
git clone <URL-del-repo>
```

1. Abrir el proyecto con **Unity Hub** apuntando a la carpeta raíz clonada. Unity restaura
   `Library/` automáticamente (la primera importación tarda varios minutos).
2. En el **primer arranque**, la escena `Assets/Scenes/EscuelaEvacuacion.unity` se genera **sola**
   (`AutoConstruirEscena.cs`). Para **regenerarla** en cualquier momento, usar el menú:
   **Herramientas → Evacuation Trainer → Construir Escena Escuela (2 pisos)**.
   El constructor crea y cablea todo automáticamente: edificio de 2 pisos, fuego, señalización,
   teleportación, XR Origin, HUD y gestores.
3. Abrir `Assets/Scenes/EscuelaEvacuacion.unity` y pulsar **Play** con el visor conectado.
4. **Flujo de la experiencia:** apareces en el **Aula 201 (piso 2)** → a los ~12 s suena la
   **alarma** → evacúa con teleport (gatillo) siguiendo las **señales verdes**: pasillo → escalera
   de emergencia (o rampa accesible) → pasillo del piso 1 → puerta principal → **punto de encuentro**
   en el patio → se muestra tu **evaluación final sobre 20 puntos**.

---

## Controles

| Acción | Control VR | Teclado (modo escritorio, sin visor) |
|--------|-----------|--------------------------------------|
| Moverse | Empujar stick sobre área azul (teleport) | `W A S D` (+ `Shift` para correr) |
| Mirar | Girar la cabeza / stick derecho | Mantener **clic derecho** + ratón |
| Reiniciar simulacro | — | `R` |
| Alto contraste | — | `C` |
| Tamaño de subtítulos | — | `+` / `-` |
| Benchmark FPS (60 s) | Automático al sonar la alarma | `B` |

> **Modo escritorio:** si no hay visor conectado, `ControlEscritorio.cs` se activa solo al dar
> Play (vía `RuntimeInitializeOnLoadMethod`): caminas con WASD y miras con clic derecho + ratón.
> No requiere configurar nada.

---

## Estructura del proyecto

```
Evacuation Trainer Inclusivo/
├── Assets/
│   ├── Editor/
│   │   ├── ConstructorEscuela.cs        Genera y cablea toda la escena por código
│   │   ├── AutoConstruirEscena.cs       Construye la escena en el primer arranque
│   │   └── GeneradorTexturasSenales.cs  Texturas de señalización por código
│   ├── Scripts/
│   │   ├── GestorEvacuacion.cs          Máquina de estados del simulacro
│   │   ├── TeleportSystem.cs            Teleport programático + confort VR
│   │   ├── EvaluadorDecisiones.cs       Registro de decisiones y nota /20
│   │   ├── PuntoDecision.cs             Checkpoints correctos/incorrectos
│   │   ├── ZonaPeligro.cs               Focos de fuego/humo y penalización
│   │   ├── SalidaSegura.cs              Detección de llegada al punto de encuentro
│   │   ├── AlarmaIncendio.cs            Sirena procedural + luces de emergencia
│   │   ├── AccesibilidadManager.cs      Subtítulos, contraste, cues de audio
│   │   ├── HUDEvacuacion.cs             HUD world-space (estado, cronómetro, reporte)
│   │   ├── SenalEvacuacion.cs           Pulso emisivo de la señalización
│   │   ├── BenchmarkXR.cs               Medición de FPS (60 s)
│   │   └── ControlEscritorio.cs         Modo sin visor (WASD + ratón)
│   ├── Scenes/
│   │   └── EscuelaEvacuacion.unity      Escena principal
│   ├── Materiales/ · Senales/           Materiales y texturas generados
│   └── Samples/ · XR/ · TextMesh Pro/   Assets del template VR (XRIT, OpenXR, TMP)
├── docs/
│   ├── arquitectura.md                  Diagrama y tabla de scripts
│   ├── BUGS.md                          Registro de bugs cerrados
│   └── capturas/                        Screenshots del proyecto
├── PLAN_PRUEBAS.md                      Plan de pruebas (8 casos)
└── README.md
```

---

## Arquitectura

Diagrama completo y tabla detallada de scripts en [`docs/arquitectura.md`](docs/arquitectura.md).

**Jerarquía de la escena:**

```
EscuelaEvacuacion.unity
├── == EDIFICIO ESCUELA ==      (piso 1, piso 2, escalera, rampa accesible, ascensor,
│                                señales, mobiliario, iluminación)
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

**Flujo (máquina de estados):**

```
   Play
    │
    ▼
[Preparacion] ──12 s──► [Alarma] ──3 s──► [Evacuacion] ──llega al patio──► [Finalizado]
 subtítulo de           sirena +           7 puntos de                      reporte /20
 bienvenida             luces rojas +      decisión +                       en el HUD
 (Aula 201, P2)         fuego activo       3 zonas de peligro                  │
    ▲                                                                          │ tecla R
    └──────────────────────────── ReiniciarSimulacro() ◄──────────────────────┘
```

**Decisiones técnicas clave:**

1. **Detección por proximidad, no por triggers:** la teleportación de XRIT no genera eventos
   `OnTriggerEnter` confiables (ver `docs/BUGS.md`, BUG-001). Los checkpoints comparan distancia
   XZ entre la cámara y su posición — robusto y más barato que la física.
2. **Audio 100% procedural:** sirena y cues se generan con `AudioClip.Create`, sin dependencias de
   assets externos y manteniendo el repositorio liviano.
3. **Escena generada por código de editor:** `ConstructorEscuela.cs` construye y cablea toda la
   escena de forma determinista y re-ejecutable; cualquier integrante reproduce la escena exacta
   con un clic (clave para trabajar en equipo con escenas binarias).
4. **Singletons + eventos C#:** los sistemas se localizan entre sí sin referencias arrastradas a
   mano en el inspector, evitando referencias rotas al regenerar la escena.

---

## Accesibilidad (lo "Inclusivo")

- **4 vías de salida y 2 puntos de reunión** (referencia RNE A.130: vías de evacuación redundantes
  en extremos opuestos): puerta principal (oeste), salida de emergencia este (junto a la escalera),
  salida de emergencia sur (Aula 101) y la rampa accesible (piso 2). Cualquiera de las 4 lleva a un
  punto de reunión válido.
- **Rampa de evacuación accesible** (referencia RNE A.120 Perú): dos tramos de pendiente suave
  (~11 %) con descanso intermedio y pasamanos, desde el Aula 201 hasta el nivel del patio. Ruta de
  evacuación completa y válida para usuarios en silla de ruedas, alternativa a la escalera, con su
  propia señalización «RUTA ACCESIBLE».
- **Señalización según NTP 399.010-1** (Perú): «RUTA DE EVACUACIÓN», «SALIDA», «ZONA SEGURA EN CASO
  DE SISMOS» (placa S), «PUNTO DE REUNIÓN», extintores señalizados y prohibición de ascensor.
- **Subtítulos** de todos los eventos sonoros con fondo de alto contraste y tamaño ajustable.
- **Cues de audio** diferenciados: triple beep agudo = alerta, beep suave = información, clic corto
  = confirmación de teleport (apoyo para usuarios con baja visión).
- **Modo alto contraste**: texto amarillo sobre negro y señales de evacuación con el doble de brillo.
- **Confort VR**: locomoción exclusivamente por teleportación + viñeta de túnel activable
  (`TeleportSystem.SetModoComodidad`), reduciendo el cybersickness.

---

## Capturas

_(agregar mínimo 3 screenshots reales en `docs/capturas/` y referenciarlas aquí)_

| Aula 201 (inicio) | Evacuación con fuego | Reporte final |
|---|---|---|
| ![inicio](docs/capturas/01_inicio.png) | ![fuego](docs/capturas/02_fuego.png) | ![reporte](docs/capturas/03_reporte.png) |

---

## Video demo

_(agregar link de YouTube no listado o Google Drive)_

---

## Resultados de pruebas

- **Plan de pruebas:** [`PLAN_PRUEBAS.md`](PLAN_PRUEBAS.md) — 8 casos de prueba (TC-01 a TC-08).
- **Bugs documentados y cerrados:** [`docs/BUGS.md`](docs/BUGS.md).
- **FPS promedio medido:** _(ejecutar `BenchmarkXR` — tecla `B` o automático con la alarma — y
  copiar aquí el resultado de `benchmark_xr.txt`)_. Objetivo VR: ≥ 72 FPS.

| Métrica | Valor medido | Objetivo |
|---------|--------------|----------|
| FPS promedio | _(benchmark)_ | ≥ 72 |
| FPS 1% low | _(benchmark)_ | ≥ 60 |
| Draw calls (Stats) | _(anotar)_ | < 300 |
| Memoria (Profiler) | _(anotar)_ | < 2 GB |

> Optimización aplicada si FPS < 72: desactivar sombras de la luz direccional
> (*Edit → Project Settings → Quality → Shadows: Disable*) y reducir los focos puntuales.
