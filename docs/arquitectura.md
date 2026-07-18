# Arquitectura Técnica — Evacuation Trainer Inclusivo (VR)

## Stack

| Capa | Tecnología |
|------|-----------|
| Motor | Unity 6000.5.3f1 + URP 17.5 |
| XR | OpenXR 1.17.1 + XR Interaction Toolkit 3.4.1 (Starter Assets) |
| UI | TextMeshPro (señales 3D + HUD world-space) |
| Input | Input System 1.19 (acciones XRI + atajos de teclado para demo) |
| Audio | Generado 100% por código (`AudioClip.Create`): sirena y cues de accesibilidad |

## Jerarquía de escena (`EscuelaEvacuacion.unity`)

```
== EDIFICIO ESCUELA ==
├── Terreno y Patio          (césped, veredas, 2 PUNTOS DE REUNIÓN con SalidaSegura)
├── Piso 1                   (pasillo central + Aula 101/102, Laboratorio, Aula 103, Dirección)
├── Piso 2                   (Aula 201 ← inicio, Aula 202, Biblioteca, Aula 203, Sala Profesores)
├── Escalera de Emergencia   (rampa + 10 peldaños + barandas; conecta pisos)
├── Rampa Accesible          (RNE A.120: 2 tramos ~11% + descanso + pasamanos, ruta alternativa)
├── Ascensor                 (ruta INCORRECTA, señalizada en rojo)
├── Senales de Evacuacion    (señalización peruana NTP 399.010: rutas, S de zona segura, extintores)
├── Mobiliario               (pizarras + 4 carpetas con silla por ambiente)
└── Lamparas                 (plafones emisivos + focos puntuales)

== JUGABILIDAD ==
├── Zonas de Peligro         (3 × ZonaPeligro: humo P2 oeste, fuego P1 este, laboratorio)
├── Puntos de Decision       (7 × PuntoDecision: 4 correctos, 3 incorrectos)
├── Teleportacion            (16 Teleport Areas + 3 Teleport Anchors en la escalera)
├── PuntoInicio              (Aula 201, piso 2)
└── == GESTORES ==
    ├── GestorEvacuacion     (máquina de estados)
    ├── EvaluadorDecisiones  (puntaje /20)
    ├── AccesibilidadManager (subtítulos, contraste, cues)
    ├── TeleportSystem       (teleport por código + confort)
    ├── BenchmarkXR          (medición FPS)
    └── Alarma de Incendio   (AlarmaIncendio + 4 luces de emergencia)

XR Origin (XR Rig)           (prefab XRIT: cámara, interactors, TeleportationProvider, viñeta)
HUD Evacuacion               (canvas world-space: estado, cronómetro, subtítulos, reporte)
Luz Direccional (Sol)
```

## Tabla de scripts

| Script | Responsabilidad | Patrón |
|--------|-----------------|--------|
| `GestorEvacuacion.cs` | Máquina de estados Preparacion→Alarma→Evacuacion→Finalizado; cronómetro; orquesta alarma, peligros y reinicio | Singleton + eventos C# |
| `TeleportSystem.cs` | Teleport programático vía `TeleportationProvider.QueueTeleportRequest`; punto de inicio; modo comodidad (viñeta) | Singleton, fachada sobre XRIT |
| `EvaluadorDecisiones.cs` | Registra decisiones y contactos con peligro; calcula nota /20 y genera reporte | Singleton |
| `PuntoDecision.cs` | Checkpoint correcto/incorrecto por proximidad de cabeza (1 registro por intento) | Registro estático |
| `ZonaPeligro.cs` | Fuego/humo dormido hasta la alarma; penaliza por proximidad con cooldown | Registro estático |
| `SalidaSegura.cs` | Detecta llegada al punto de encuentro y finaliza el simulacro | — |
| `AlarmaIncendio.cs` | Sirena generada por código (barrido 550–950 Hz) + parpadeo de luces rojas | — |
| `AccesibilidadManager.cs` | Subtítulos con fondo, alto contraste, escala de texto, cues de audio | Singleton |
| `HUDEvacuacion.cs` | Canvas world-space que sigue la mirada; estado, cronómetro y reporte final | Observador de eventos |
| `SenalEvacuacion.cs` | Pulso emisivo de señales; ×2.2 de brillo en alto contraste | — |
| `BenchmarkXR.cs` | 60 s de medición: FPS promedio, min, max, 1% low; guarda .txt | — |
| `ControlEscritorio.cs` | Modo sin visor: WASD + ratón sobre el XR Origin; se auto-instala al dar Play solo si no hay casco activo | `RuntimeInitializeOnLoadMethod` |
| `ConstructorEscuela.cs` (Editor) | Genera toda la escena por código: geometría, materiales, teleport, wiring | Menú de editor |

## Flujo de la aplicación

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

## Decisiones técnicas clave

1. **Detección por proximidad, no por triggers:** la teleportación XRIT no genera eventos
   `OnTriggerEnter` confiables (ver BUG-001). Todos los checkpoints comparan distancia XZ entre
   `Camera.main` y su posición — robusto y más barato que física.
2. **Audio 100% procedural:** sirena y cues se generan con `AudioClip.Create`, eliminando
   dependencias de assets externos y manteniendo el repo liviano.
3. **Escena generada por código de editor:** `ConstructorEscuela.cs` construye y cablea toda la
   escena de forma determinista y re-ejecutable; cualquier integrante reproduce la escena
   exacta con un clic (clave para trabajo en equipo con escenas binarias).
4. **Singletons + eventos:** los sistemas se encuentran entre sí sin referencias arrastradas a
   mano en el inspector, lo que evita referencias rotas al regenerar la escena.

## Métricas de rendimiento (completar tras TC-07)

| Métrica | Valor medido | Objetivo |
|---------|--------------|----------|
| FPS promedio | _(benchmark)_ | ≥ 72 |
| FPS 1% low | _(benchmark)_ | ≥ 60 |
| Draw calls (Stats) | _(anotar)_ | < 300 |
| Memoria (Profiler) | _(anotar)_ | < 2 GB |

Optimización aplicada si FPS < 72: desactivar sombras de la luz direccional
(*Edit → Project Settings → Quality → Shadows: Disable*) y reducir `Focos` puntuales.
