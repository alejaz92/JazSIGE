# Mejoras Implementadas y Recomendaciones Futuras

## Mejoras Implementadas (Enero 2026)

### Documentación
- ✅ Actualizado README.md con nuevos endpoints de diagnóstico (`wsfe/param/tipos-cbte`, `wsfe/comp/ultimo-autorizado`, `wsfe/cae/hardcoded`)
- ✅ Ampliadas notas sobre idempotencia, validaciones, ambiente, QR URL y notas de crédito/débito

### Comentarios de Código
- ✅ Documentado completamente `FiscalDocumentService.CreateAsync()` con 9 pasos numerados
- ✅ Agregados comentarios detallados en métodos `BuildWsfeCaeRequest()`, `BuildArcaRequest()`, `GenerateInvoiceNumber()`
- ✅ Documentado método `GenerateAfipQrUrl()` y estructura `QrData`
- ✅ Mejorada documentación en `ValidateBuyerDataByInvoiceType()`

### XML Docs (Comentarios de Documentación)
- ✅ Agregados resúmenes XML Doc a:
  - `FiscalDocumentController` (clase y 6 endpoints)
  - `FiscalDocumentDTO` (clase y todas las propiedades)
  - `FiscalDocumentCreateDTO` y `FiscalDocumentItemDTO` (clase y propiedades)
  - `FiscalDocument` (entidad de BD y todas las propiedades)
  - `FiscalDocumentItem` (entidad de BD y propiedades)

### Manejo de Errores
- ✅ Mejorado manejo de errores en `FiscalDocumentController.Create()`:
  - Validación de null en request body
  - Validación de items vacíos
  - Múltiples tipos de excepción con códigos específicos (VALIDATION_ERROR, CONFIGURATION_ERROR, INTERNAL_ERROR)

### Mejoras en Controladores
- ✅ Mejorados parámetros de error en endpoints `credit-notes` y `debit-notes` (cambio de "relatedId" a "saleId")
- ✅ Agregadas validaciones explícitas de parámetros

---

## Recomendaciones Futuras

### 1. Logging y Observabilidad
- Implementar ILogger en `FiscalDocumentService` para:
  - Log de inicio/fin de operaciones ARCA
  - Log de errores con ArcaCorrelationId para trazabilidad
  - Log de migraciones de estado (Pending → Authorized/Rejected)
- Agregar Application Insights o similar para monitoreo en producción

### 2. Patrones de Resiliencia
- Implementar Polly para reintentos automáticos en llamadas a ARCA
- Agregar circuit breaker para fallos de conexión con WSFE
- Implementar timeout personalizado por tipo de operación

### 3. Caché Mejorado
- Expandir `IArcaAccessTicketCache` para cachear:
  - Parámetros de ARCA (tipos de comprobante, tipos de documento, alícuotas IVA)
  - Último número de comprobante autorizado por POS/tipo
- Implementar invalidación de caché inteligente

### 4. Validación de Datos
- Crear validator específico usando FluentValidation para:
  - `FiscalDocumentCreateDTO` (totales, rangos, formatos)
  - Validaciones cruzadas entre campos
  - Mensajes de error localizables
- Agregar validación de CUIT/DNI usando algoritmos de dígito verificador

### 5. Auditoría Mejorada
- Implementar tabla de auditoría para rastrear cambios de ArcaStatus
- Agregar campos: `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`
- Registrar quién autorizó/rechazó documentos
- Implementar soft delete si es necesario para documentos rechazados

### 6. Manejo de Estados Más Granular
- Expandir `ArcaStatus` con más estados:
  - `Pending` → `SendingToArca` → `WaitingForResponse` → `Authorized` / `Rejected` / `Unknown`
  - Agregar timeout para estados "en vuelo"
  - Implementar mecanismo de reconciliación con ARCA

### 7. Integración con Externos
- Webhook para notificar al servicio de Órdenes de Venta cuando una factura es autorizada
- Exponer endpoint para consultar estado de autorización
- Agregar endpoint para descargar XML de comprobante desde ARCA

### 8. Testing
- Agregar unit tests para `FiscalDocumentService` (mocking de clientes externos)
- Tests de integración contra WSFE homologación
- Fixtures con casos de prueba para diferentes tipos de facturas

### 9. Documentación Externa
- Crear OpenAPI/Swagger con ejemplos reales
- Documentar códigos de error ARCA comunes y soluciones
- Guía para migración Dummy → WSFE real
- Cheat sheet de códigos ARCA (tipos documento, tipos comprobante, alícuotas IVA)

### 10. Performance
- Analizar N+1 queries en repositorio de `FiscalDocument` (eager loading de Items)
- Implementar pagination para endpoints de listado
- Considerar índices de BD para búsquedas por SalesOrderId, DocumentNumber, etc.
- Caché de resultados GET con headers Cache-Control

### 11. Seguridad
- Validar que JWT include claims apropiados (CompanyId, UserId, Role)
- Implementar autorización a nivel de recurso (usuario solo ve docs de su compañía)
- Agregar rate limiting en endpoints de creación
- Auditar acceso a datos sensibles (CAE, CUIT, montos)

### 12. Refactorización de Código
- Extraer lógica de validación de totales a clase `FiscalDocumentValidator`
- Extraer mapeo a DTOs a clase `FiscalDocumentMapper`
- Crear `IFiscalDocumentAuthorizationStrategy` para abstraer dummy vs WSFE
- Consolidar conversión de tipos de documento en enums estáticos

---

## Notas sobre Código Actual

### Puntos Fuertes
✅ Estructura en capas bien definida (Controllers → Services → Repositories)
✅ Separación clara entre Dummy (dev) y WSFE (prod)
✅ Auditoría integrada en modelo (ArcaStatus, ArcaLastInteractionAt, ArcaCorrelationId)
✅ Almacenamiento de payloads JSON para debugging
✅ Idempotencia en invoices

### Áreas de Mejora Identificadas
- Error handling podría ser más específico (diferenciador entre ARCA errors vs config errors)
- No hay logging estructurado
- Tests no encontrados en workspace
- Documentación técnica era mínima (ahora mejorada)
- Validaciones podrían ser más exhaustivas (especialmente formatos de documento)

---

## Ejecución Recomendada de Mejoras

1. **Corto plazo** (1-2 sprints): Logging, validación con FluentValidation, tests básicos
2. **Mediano plazo** (3-4 sprints): Resiliencia (Polly), caché mejorada, auditoría tabla
3. **Largo plazo**: Webhook, oficialización de documentación, performance optimization
