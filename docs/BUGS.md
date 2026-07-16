# Registro de Bugs — Evacuation Trainer Inclusivo (VR)

Formato: severidad CRÍTICO / MAYOR / MENOR. Cada bug cerrado referencia su corrección.

---

## BUG-001 · Los triggers de física no detectaban al jugador tras un teleport — CERRADO
- **Severidad:** CRÍTICO (bloqueaba toda la evaluación de decisiones)
- **Síntoma:** al teletransportarse dentro de una zona de peligro o punto de decisión,
  `OnTriggerEnter` no se disparaba y el evaluador no registraba nada.
- **Causa raíz:** la teleportación de XRIT mueve el `XR Origin` reposicionando el transform;
  sin un `Rigidbody` en movimiento, PhysX no genera eventos de trigger de forma confiable.
- **Corrección:** se reemplazó la detección por triggers por **detección por proximidad de la
  cabeza** (distancia XZ entre `Camera.main` y el punto, comprobada en `Update`). Implementado en
  `ZonaPeligro.cs`, `PuntoDecision.cs` y `SalidaSegura.cs`.
- **Verificación:** TC-02 y TC-04 del plan de pruebas.

## BUG-002 · Texto de las señales deformado — CERRADO
- **Severidad:** MENOR (visual)
- **Síntoma:** el texto "SALIDA →" aparecía estirado/aplastado en algunas señales.
- **Causa raíz:** el TextMeshPro era hijo de la placa (cubo con escala no uniforme
  `1.1 × 0.45 × 0.05`); la escala del padre deformaba el mesh del texto.
- **Corrección:** en `ConstructorEscuela.Senal()` el texto se crea como **hermano** de la placa,
  posicionado 3.5 cm delante de su cara visible, nunca como hijo del cubo escalado.
- **Verificación:** inspección visual de las 11 señales en ambos pisos.

## BUG-003 · Señales de los muros este/oeste miraban hacia dentro de la pared — CERRADO
- **Severidad:** MAYOR (el usuario no veía la señal de la salida principal)
- **Síntoma:** el cartel "SALIDA" sobre la puerta principal y el "↓ BAJA POR LA ESCALERA" del
  muro este no eran visibles: la cara con texto quedaba dentro del muro.
- **Causa raíz:** rotación Y invertida: `Quaternion.Euler(0, 90, 0)` orienta el forward hacia +X,
  no hacia −X como se asumió al colocar señales en muros verticales.
- **Corrección:** se corrigieron las rotaciones en `ConstructorEscuela.ConstruirSenales()`
  (muro oeste → 90°, muro este → −90°).
- **Verificación:** TC-02 (las señales guían la ruta completa).

## BUG-004 · _(plantilla para bugs encontrados en pruebas)_
- **Severidad:**
- **Síntoma:**
- **Causa raíz:**
- **Corrección:** (commit `fix: ... — BUG-004`)
- **Verificación:**
