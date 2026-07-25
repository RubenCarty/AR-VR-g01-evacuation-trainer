# Plan de Pruebas — Evacuation Trainer Inclusivo (VR)

**Grupo G01 · Semana 15-16.** Ejecutar cada caso en Play mode (visor OpenXR o XR Device Simulator)
y marcar el estado real. Un caso PASA solo si el resultado observado coincide con el esperado.

## Casos de prueba

### TC-01 · Flujo de inicio y alarma
- **Precondición:** escena `EscuelaEvacuacion` abierta, Play mode recién iniciado.
- **Pasos:** 1) No moverse. 2) Esperar 12 segundos.
- **Resultado esperado:** el usuario aparece en el Aula 201 (piso 2) con subtítulo de bienvenida;
  a los 12 s suena la sirena, parpadean luces rojas, aparecen fuego/humo y el subtítulo
  "¡ALARMA DE INCENDIO!". El cronómetro del HUD empieza a correr.
- **Estado:** ☐ PASA ☐ FALLA

### TC-02 · Teleportación completa de la ruta segura
- **Precondición:** TC-01 ejecutado, alarma activa.
- **Pasos:** 1) Teleportarse por el pasillo del piso 2 hacia el este. 2) Bajar por las 3 anclas de
  la escalera. 3) Recorrer el pasillo del piso 1 hacia el oeste. 4) Salir por la puerta principal
  hasta el punto de encuentro.
- **Resultado esperado:** todas las áreas azules aceptan teleport; cada teleport reproduce el cue
  de audio; al llegar al punto de encuentro el simulacro finaliza y el HUD muestra el reporte.
- **Estado:** ☐ PASA ☐ FALLA

### TC-03 · Evaluación de decisiones correctas
- **Precondición:** simulacro reiniciado (`R`).
- **Pasos:** completar la ruta segura sin acercarse al ascensor ni al fuego, en menos de 120 s.
- **Resultado esperado:** reporte final con 4 decisiones correctas, 0 incorrectas, 0 contactos
  con peligro y **nota 20/20**.
- **Estado:** ☐ PASA ☐ FALLA

### TC-04 · Penalización por ascensor y fuego
- **Precondición:** simulacro reiniciado, alarma activa.
- **Pasos:** 1) Acercarse a la puerta del ascensor (piso 2). 2) Entrar al humo del pasillo oeste.
  3) Completar luego la ruta segura.
- **Resultado esperado:** subtítulos de alerta "✘ Decisión incorrecta…" y "¡Peligro!…"; el reporte
  final descuenta 2 pts por decisión incorrecta y 3 pts por contacto con peligro
  (ej.: 2 incorrectas + 1 contacto = nota ≤ 13).
- **Estado:** ☐ PASA ☐ FALLA

### TC-05 · Accesibilidad: subtítulos, contraste y cues
- **Precondición:** Play mode activo.
- **Pasos:** 1) Pulsar `C` (alto contraste). 2) Pulsar `+` dos veces. 3) Provocar una alerta
  (acercarse al fuego).
- **Resultado esperado:** el subtítulo pasa a amarillo sobre fondo negro casi opaco; el texto crece;
  las señales verdes brillan notoriamente más; la alerta suena con triple beep agudo distinto del
  beep suave informativo.
- **Estado:** ☐ PASA ☐ FALLA

### TC-06 · Reinicio del simulacro
- **Precondición:** simulacro finalizado (reporte visible).
- **Pasos:** pulsar `R`.
- **Resultado esperado:** el usuario vuelve al Aula 201, el fuego se apaga, el cronómetro se
  reinicia, los puntos de decisión vuelven a estar activos y la alarma vuelve a sonar a los 12 s.
- **Estado:** ☐ PASA ☐ FALLA

### TC-07 · Rendimiento (Benchmark 60 s)
- **Precondición:** visor conectado, alarma activa (el benchmark arranca solo) o pulsar `B`.
- **Pasos:** evacuar normalmente durante los 60 s de medición.
- **Resultado esperado:** al terminar aparece el subtítulo con FPS promedio y 1% low; el resultado
  queda en la consola y en `%userprofile%\AppData\LocalLow\...\benchmark_xr.txt`.
  **Criterio: FPS promedio ≥ 72.**
- **FPS obtenido:** ______  **1% low:** ______
- **Estado:** ☐ PASA ☐ FALLA

### TC-08 · Estabilidad 10 minutos
- **Precondición:** Play mode activo.
- **Pasos:** completar el simulacro 3 veces seguidas (usando `R`) y dejar la app corriendo 10 min.
- **Resultado esperado:** sin crashes, sin errores en consola, sin fugas visibles de memoria
  (Profiler estable).
- **Estado:** ☐ PASA ☐ FALLA

## Resumen

| TC | Descripción corta | Estado |
|----|-------------------|--------|
| TC-01 | Inicio y alarma | ☐ |
| TC-02 | Ruta segura completa | ☐ |
| TC-03 | Nota perfecta | ☐ |
| TC-04 | Penalizaciones | ☐ |
| TC-05 | Accesibilidad | ☐ |
| TC-06 | Reinicio | ☐ |
| TC-07 | Benchmark FPS | ☐ |
| TC-08 | Estabilidad 10 min | ☐ |
