# Fiscal Documentation Service

Microservicio encargado de recibir solicitudes de generación de facturas, notas de débito y crédito, y realizar la gestión y autorización con el ente argentino ARCA (ex AFIP).

## Estructura del Proyecto

El proyecto está organizado siguiendo una arquitectura en capas:

```
FiscalDocumentationService/
├── Business/                    # Lógica de negocio
│   ├── Exceptions/             # Excepciones personalizadas
│   ├── Interfaces/             # Interfaces de servicios y clientes
│   │   ├── Clients/            # Interfaces de clientes externos
│   │   │   └── Dummy/          # Interfaces de servicios dummy/mock
│   ├── Middlewares/            # Middlewares de la aplicación
│   ├── Models/                 # DTOs y modelos de negocio
│   │   ├── Arca/              # Modelos relacionados con ARCA
│   │   ├── Clients/           # Modelos de clientes externos
│   │   └── Diagnostics/       # Modelos de diagnóstico
│   ├── Options/               # Opciones de configuración
│   └── Services/              # Servicios de negocio
│       └── Clients/           # Clientes de servicios externos
│           ├── Dummy/         # Servicios dummy/mock (solo desarrollo/testing)
│           └── [Real ARCA clients]
├── Controllers/                # Controladores de la API
├── Infrastructure/             # Capa de infraestructura
│   ├── Data/                  # DbContext y configuración de EF
│   ├── Interfaces/            # Interfaces de repositorios
│   ├── Models/                # Entidades de base de datos
│   └── Repositories/          # Implementación de repositorios
└── Program.cs                  # Configuración de la aplicación
```

## Servicios Dummy vs Servicios Reales

### Servicios Dummy (Desarrollo/Testing)

Los servicios ubicados en `Business/Services/Clients/Dummy/` son implementaciones que **simulan** la funcionalidad sin contactar realmente con ARCA. Estos servicios son útiles para:

- Desarrollo local sin necesidad de certificados ARCA
- Testing sin depender de servicios externos
- Pruebas de integración sin costos de facturación real

**Servicios Dummy disponibles:**
- `DummyArcaServiceClient`: Simula la autorización de facturas, siempre retornando aprobación con un CAE aleatorio.

### Servicios Reales (Producción)

Los servicios ubicados directamente en `Business/Services/Clients/` son implementaciones reales que contactan con ARCA:

- `ArcaAuthClient`: Maneja la autenticación con WSAA (Web Service de Autenticación y Autorización)
- `ArcaWsfeClient`: Cliente para el Web Service de Facturación Electrónica (WSFE)

## Configuración

### Habilitar/Deshabilitar ARCA

El servicio utiliza la configuración de la compañía para determinar si debe usar servicios dummy o reales:

- **ARCA Deshabilitado**: Usa `DummyArcaServiceClient` para simular la autorización
- **ARCA Habilitado**: Usa `ArcaWsfeClient` para contactar realmente con ARCA

Esta configuración se obtiene del servicio de compañía (`CompanyServiceClient`) mediante el campo `ArcaEnabled` en `CompanyFiscalSettingsDTO`.

### Configuración de ARCA

La configuración de ARCA se realiza en `appsettings.json` bajo la sección `Arca`:

```json
{
  "Arca": {
    "Environment": "Homologation",  // "Homologation" o "Production"
    "AllowProductionEmission": false,
    "Wsaa": {
      "HomologationUrl": "...",
      "ProductionUrl": "..."
    },
    "Wsfe": {
      "HomologationUrl": "...",
      "ProductionUrl": "..."
    },
    "Certificate": {
      "PfxPath": "path/to/certificate.pfx",
      "PfxPassword": "password"
    }
  }
}
```

## Uso del Microservicio

### Crear una Factura

```http
POST /api/FiscalDocument
Authorization: Bearer {token}
Content-Type: application/json

{
  "invoiceType": 1,
  "buyerDocumentType": 80,
  "buyerDocumentNumber": 20123456789,
  "salesOrderId": 123,
  "totalAmount": 1000.00,
  "netAmount": 826.45,
  "vatAmount": 173.55,
  "exemptAmount": 0,
  "nonTaxableAmount": 0,
  "otherTaxesAmount": 0,
  "currency": "PES",
  "exchangeRate": 1,
  "receiverVatConditionId": 1,
  "items": [
    {
      "sku": "PROD001",
      "description": "Producto de prueba",
      "unitPrice": 1000.00,
      "quantity": 1,
      "vatId": 5,
      "vatBase": 826.45,
      "vatAmount": 173.55
    }
  ]
}
```

### Obtener una Factura por ID

```http
GET /api/FiscalDocument/{id}
Authorization: Bearer {token}
```

### Obtener Facturas por Orden de Venta

```http
GET /api/FiscalDocument/by-sales-order/{salesOrderId}
Authorization: Bearer {token}
```

## Endpoints de Diagnóstico

El servicio incluye endpoints de diagnóstico para verificar la conectividad con ARCA:

- `GET /api/Diagnostics/wsaa`: Prueba la conexión con WSAA (obtiene ticket de acceso)
- `GET /api/Diagnostics/wsfe/dummy`: Prueba la conexión básica con WSFE (FEDummy)
- `GET /api/Diagnostics/wsfe/param/tipos-cbte`: Obtiene los tipos de comprobantes disponibles en ARCA
- `GET /api/Diagnostics/wsfe/comp/ultimo-autorizado?cbteType=1`: Obtiene el último número de comprobante autorizado para un tipo específico
- `POST /api/Diagnostics/wsfe/cae/hardcoded`: Genera una factura hardcodeada de prueba para verificar todo el flujo de generación de CAE

## Notas Importantes

1. **Servicios Dummy**: Los servicios en la carpeta `Dummy/` están claramente identificados y documentados. Solo deben usarse en desarrollo/testing.

2. **Idempotencia**: El servicio implementa idempotencia para facturas basándose en `SalesOrderId`. Si ya existe una factura para una orden de venta, retorna la existente. Las notas de crédito y débito no son idempotentes.

3. **Validaciones**: El servicio valida:
   - Totales de importes (neto + IVA + exento + no gravado + otros impuestos = total)
   - Datos del comprador según el tipo de factura
   - Configuración de ARCA cuando está habilitado
   - Campos requeridos para notas de crédito/débito (ReferencedInvoiceType, ReferencedPointOfSale, ReferencedInvoiceNumber)
   - Formato y longitud de documentos según tipo de comprobante

4. **Ambiente**: El servicio valida que el ambiente configurado en `Arca:Environment` coincida con el configurado en el servicio de compañía.

5. **QR URL**: Cada respuesta incluye un `ArcaQrUrl` generado según el estándar AFIP/ARCA para facilitar la impresión de comprobantes con código QR.

6. **Notas de Crédito y Débito**: Requieren referencia a un comprobante original (invoice). Deben proporcionar:
   - `ReferencedInvoiceType`: Tipo del comprobante original (ej: 1 para Factura A, 6 para Factura B)
   - `ReferencedPointOfSale`: Punto de venta del comprobante original
   - `ReferencedInvoiceNumber`: Número del comprobante original

## Dependencias Principales

- .NET 9.0
- Entity Framework Core 9.0
- ASP.NET Core 9.0
- JWT Bearer Authentication
- Swagger/OpenAPI

## Desarrollo

Para desarrollo local sin ARCA, asegúrate de que `ArcaEnabled` esté en `false` en la configuración de la compañía. Esto hará que el servicio use `DummyArcaServiceClient` automáticamente.
