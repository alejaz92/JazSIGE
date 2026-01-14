# Documentación de la solución JazSIGE

Resumen general
---------------
Esta solución contiene varios microservicios .NET organizados por dominio. Está preparada para ejecución local y despliegue en contenedores/entornos cloud. Requiere .NET SDK9 (algunos proyectos pueden targetear .NET8/9). A continuación se describen los proyectos principales, estructura y notas prácticas.

Prerequisitos
-------------
- .NET9 SDK (usar `dotnet --version` para verificar)
- Bases de datos y settings por proyecto (revisar `appsettings.json` en cada proyecto)
- (Opcional) Docker si se desea ejecutar servicios en contenedores

Cómo ejecutar un proyecto localmente
-----------------------------------
Desde la raíz de la solución puedes ejecutar cualquier proyecto con:

`dotnet run --project {ruta-al-proyecto}`

Ejemplo:

`dotnet run --project FiscalDocumentationService/FiscalDocumentationService.csproj`

Resumen de proyectos
--------------------
Se listan los proyectos encontrados en la solución con una descripción funcional, estructura típica y notas relevantes.

1) GatewayService
 - Ruta: `GatewayService/GatewayService.csproj`
 - Propósito: API Gateway / enrutamiento (probablemente usando Ocelot o ASP.NET Core proxy).
 - Estructura típica: `Program.cs`, configuración de ruteo, autenticación y agregación de servicios.
 - Nota: revisar configuración de endpoints y políticas CORS/auth en `Program.cs`.

2) AuthService
 - Ruta: `AuthService/AuthService.csproj`
 - Propósito: Emisión y validación de tokens JWT, gestión de usuarios/roles.
 - Estructura típica: controladores de autenticación, servicios de Identity, repositorios.
 - Nota: revisar settings de token y secretos en `appsettings.json`.

3) CatalogService
 - Ruta: `CatalogService/CatalogService.csproj`
 - Propósito: Gestión del catálogo de productos/artículos.
 - Estructura típica: controladores, servicios de dominio, repositorios, modelos DTO/Entities.

4) CompanyService
 - Ruta: `CompanyService/CompanyService.csproj`
 - Propósito: Configuración y datos de la compañía (datos fiscales, configuración ARCA, direcciones, etc.).
 - Uso frecuente: otros microservicios consultan este servicio para obtener `CompanyFiscalSettings`.

5) StockService
 - Ruta: `StockService/StockService.csproj`
 - Propósito: Gestión de inventario, movimientos de stock, reservas y entregas.

6) FiscalDocumentationService
 - Ruta: `FiscalDocumentationService/FiscalDocumentationService.csproj`
 - Propósito: Generación y autorización de documentos fiscales (facturas, notas de crédito/débito) con integración al ente ARCA (ex AFIP).
 - Detalles existents en `FiscalDocumentationService/README.md` dentro del proyecto.
 - Puntos clave:
 - Implementa flujo "Dummy" para desarrollo y flujo WSFE real para producción.
 - Valida totales, idempotencia por `SalesOrderId`, y datos del emisor/recibidor.
 - Configuración ARCA via `appsettings.json` sección `Arca`.
 - Clientes externos: `CompanyService` (para settings de compañía), `DummyArcaServiceClient` y `ArcaWsfeClient`.

7) SalesService
 - Ruta: `SalesService/SalesService.csproj`
 - Propósito: Gestión de ventas, órdenes, y su relación con facturación.
 - Interacciones: suele llamar a `FiscalDocumentationService` para generar documentos fiscales.

8) PurchaseService
 - Ruta: `PurchaseService/PurchaseService.csproj`
 - Propósito: Gestión de compras y documentos relacionados (facturas de proveedores, notas).
 - Puntos visibles en código:
 - `PurchaseDocumentService` gestiona creación, cancelación y notificación al servicio de Contabilidad.
 - When creating a document it calls `IAccountingServiceClient.UpsertExternalAsync` to register the invoice in Accounting.
 - So there is an integration with `AccountingService` for receipts/covering invoices.

9) AccountingService
 - Ruta: `AccountingService/AccountingService.csproj`
 - Propósito: Gestión de cuentas corrientes, recibos, y lógica contable/financiera.
 - Interacción: Provee endpoints usados por `PurchaseService` y tal vez por `SalesService`.

Patrones y convenciones observados
---------------------------------
- Arquitectura por microservicios: cada dominio en su propio proyecto.
- Separación por capas: `Business`, `Infrastructure`, `Controllers` en varios proyectos.
- Uso de DTOs, Repositories e Interfaces para desacoplar.
- Integraciones entre servicios vía clientes HTTP (interfaces `*ServiceClient` o `I*Client`).
- Validaciones y excepciones personalizadas en capa de negocio.

Configuración y secretos
------------------------
- Revisar `appsettings.json` por proyecto para conexiones DB, ARCA y otros endpoints.
- No incluir certificados ni secretos en el repositorio; usar variables de entorno o secret manager en local.

Migraciones y Base de Datos
---------------------------
- Los proyectos que usan EF Core contienen migraciones en carpetas `Migrations`.
- Ejecutar migraciones con `dotnet ef database update --project {proyecto}` y revisa `DbContext` en `Infrastructure/Data`.

Testing y entornos
------------------
- Existen clientes "Dummy" para facilitar testing local sin requerir servicios externos (por ejemplo ARCA Dummy).
- Para pruebas de integración, configurar endpoints reales y certificados donde corresponda.

Siguientes pasos recomendados
----------------------------
- Si quiere, puedo generar documentación más detallada por proyecto (endpoints expuestos, modelos DTOs, diagramas de llamadas) — especifique qué proyecto priorizar.
- También puedo generar OpenAPI/Swagger resumen si los proyectos exponen Swagger (leer `Program.cs` de cada proyecto).

Archivo creado
--------------
He creado `SOLUTION_DOCUMENTATION.md` en la raíz del repositorio con este contenido.

Si desea documentación en otro formato (README por proyecto, wiki, o exportar a Markdown por proyecto), indíquelo.