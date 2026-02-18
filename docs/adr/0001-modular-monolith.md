# ADR 0001: Adopción de Modular Monolith como arquitectura objetivo

- **Estado**: Propuesto
- **Fecha**: 2026-02-18
- **Decisores**: Arquitectura de Software, Líder Técnico de Plataforma, Product Owner

## Contexto y motivación

La solución actual está distribuida en múltiples servicios desplegados de manera independiente. Este modelo incrementa costos y fricción operativa para el tamaño y madurez actual del producto.

Motivadores principales:

1. **Costo Azure elevado**:
   - Multiplicidad de App Services, planes de cómputo, redes y monitoreo por servicio.
   - Sobrecosto en ambientes no productivos por duplicación de infraestructura.
2. **Latencia inter-servicios**:
   - Operaciones de negocio que requieren múltiples llamadas HTTP internas.
   - Penalización en p95 por encadenamiento de requests y retries.
3. **Complejidad operativa**:
   - Coordinación de despliegues entre varios repos/componentes.
   - Observabilidad fragmentada (trazas, logs y métricas en múltiples puntos).
   - Mayor superficie para fallos por contratos distribuidos y versionado de APIs internas.

## Decisión

Se define como arquitectura objetivo un **Modular Monolith**, conservando límites de dominio explícitos y contratos internos por módulo.

## Objetivos medibles (12 meses)

1. **Reducción de aplicaciones desplegadas**:
   - Baseline: cantidad actual de aplicaciones/servicios productivos desplegados de forma separada.
   - Meta: **reducir al menos 60%** la cantidad de apps desplegadas.
2. **Reducción de latencia p95 en flujos críticos**:
   - Baseline: p95 actual en flujos de venta y compra con trazas extremo a extremo.
   - Meta: **mejorar p95 en al menos 30%**.
3. **Disminución de costo mensual Azure**:
   - Baseline: costo promedio mensual de los últimos 3 meses.
   - Meta: **reducir costo total en al menos 25%** manteniendo SLAs.
4. **Eficiencia operativa de despliegue**:
   - Baseline: tiempo medio de despliegue coordinado entre servicios.
   - Meta: **reducir en al menos 40%** el lead time de release.

## Módulos iniciales

La primera versión del Modular Monolith se organiza en los siguientes módulos:

1. `Auth`
2. `Catalog`
3. `Company`
4. `Stock`
5. `Sales`
6. `Purchase`
7. `Accounting`
8. `Fiscal`

Cada módulo deberá:

- Definir su propio dominio, casos de uso y puertos de salida.
- Evitar acceso directo a persistencia de otros módulos.
- Exponer contratos internos estables para interacción entre módulos.

## NFRs obligatorios

### 1) Seguridad JWT

- Autenticación basada en JWT firmados.
- Validación obligatoria de issuer, audience, expiración y firma.
- Rotación de secretos/certificados según política de seguridad.
- Autorización por roles/permisos en casos de uso críticos.

### 2) Auditoría

- Registro auditable de operaciones sensibles (creación, actualización, anulación, aprobación).
- Persistencia de auditoría con actor, timestamp UTC, entidad, acción y diff relevante.
- Retención mínima de 12 meses para eventos críticos.

### 3) Trazabilidad

- Correlation ID obligatorio por request extremo a extremo.
- Trazas distribuidas internas por módulo con convenciones unificadas.
- Vinculación entre logs de negocio y eventos técnicos para diagnóstico.

### 4) Tiempos de respuesta

- APIs síncronas críticas con objetivo de **p95 <= 300 ms** en carga nominal.
- Consultas no críticas con objetivo de **p95 <= 800 ms**.
- Definición de presupuestos de performance por módulo y endpoint.

### 5) Disponibilidad

- Objetivo de disponibilidad mensual del sistema: **>= 99.9%**.
- Health checks obligatorios (liveness/readiness) por módulo.
- Runbooks de contingencia para degradación controlada.

## Plan de fases con criterio Go/No-Go

| Fase | Alcance | Go / No-Go (métricas objetivas) | Responsable primario |
|---|---|---|---|
| Fase 0: Baseline | Medición inicial de costos, latencia, disponibilidad y estado operativo. | **Go** si se documentan baseline de costo Azure, p95 por flujo crítico, disponibilidad mensual y número de apps desplegadas. **No-Go** si falta cualquiera de las 4 métricas. | Arquitectura + FinOps |
| Fase 1: Fundación | Estructura base del Modular Monolith, módulo Auth y estándares transversales. | **Go** si JWT, auditoría y trazabilidad están implementados y validados en entorno de staging; error budget no supera 0.1%. **No-Go** en caso contrario. | Líder Técnico Plataforma |
| Fase 2: Migración Core | Migración de Catalog, Company, Stock y Sales. | **Go** si p95 de flujos críticos mejora >= 20% vs baseline y no hay incidentes Sev1 por 30 días. **No-Go** si no cumple cualquiera. | Líder de Dominio Operaciones |
| Fase 3: Migración Backoffice | Migración de Purchase, Accounting y Fiscal. | **Go** si exactitud contable/fiscal validada al 100% en pruebas de regresión y disponibilidad >= 99.9% durante 2 ciclos de cierre. **No-Go** en caso contrario. | Líder de Finanzas TI |
| Fase 4: Optimización | Hardening, reducción de costos, simplificación de despliegues y retiro de legado. | **Go** final si reducción de apps >= 60%, reducción p95 >= 30% y costo Azure >= 25% vs baseline. **No-Go** si alguna meta no se alcanza. | CTO + Product Owner |

## Consecuencias esperadas

### Positivas

- Menor costo total de propiedad en Azure.
- Menor latencia en procesos transversales por reducción de hops de red.
- Menor complejidad de operación y release.

### Riesgos

- Riesgo de acoplamiento interno si no se respetan límites de módulo.
- Necesidad de disciplina en arquitectura y gobernanza técnica.
- Riesgo temporal de productividad durante la migración.

## Seguimiento

- Revisión mensual de métricas objetivo en comité técnico.
- Reevaluación de este ADR a los 6 meses o ante cambios regulatorios/negocio relevantes.
