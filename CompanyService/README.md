# Company Service

A microservice for managing company information used across business processes. This service provides company data for invoicing systems and business configuration management.

## Overview

The Company Service is a .NET 9.0 Web API microservice that manages the company information entity. It provides endpoints to retrieve and update company data, including fiscal settings for ARCA (Argentine tax authority) invoice generation.

## Features

- **Company Information Management**: Retrieve and update company details
- **Fiscal Settings**: Manage ARCA configuration for invoice generation
- **Catalog Integration**: Validates postal codes and IVA types against external catalog service
- **JWT Authentication**: Secure endpoints with role-based authorization
- **Global Error Handling**: Centralized exception handling middleware
- **Data Validation**: Model validation with DataAnnotations
- **Swagger Documentation**: API documentation available in development mode

## Architecture

The service follows a clean architecture pattern with the following layers:

```
CompanyService/
├── Business/              # Business logic layer
│   ├── Interfaces/       # Service interfaces
│   ├── Models/          # DTOs and business models
│   ├── Services/        # Business services
│   └── Exceptions/      # Custom exceptions
├── Infrastructure/        # Infrastructure layer
│   ├── Data/            # DbContext and database configuration
│   ├── Interfaces/     # Repository interfaces
│   ├── Models/          # Entity models
│   ├── Repositories/    # Data access implementations
│   └── Middleware/      # Custom middleware
└── Controllers/          # API controllers
```

## Prerequisites

- .NET 9.0 SDK
- SQL Server (local or Azure SQL)
- Access to Catalog Service (for postal code and IVA type validation)

## Configuration

### appsettings.json

Configure the following settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your SQL Server connection string"
  },
  "Jwt": {
    "Key": "Your JWT signing key",
    "Issuer": "JWT issuer URL",
    "Audience": "JWT audience URL"
  },
  "GatewayService": {
    "CatalogBaseUrl": "Catalog service base URL"
  }
}
```

### Environment Variables

For production, use environment variables or Azure Key Vault for sensitive configuration:
- `ConnectionStrings__DefaultConnection`
- `Jwt__Key`
- `GatewayService__CatalogBaseUrl`

## API Endpoints

### Get Company Information

```http
GET /api/CompanyInfo
```

Returns the complete company information.

**Response:**
```json
{
  "name": "Company Name",
  "shortName": "Short Name",
  "taxId": "12345678901",
  "address": "Street Address",
  "postalCodeId": 1,
  "postalCode": "1234",
  "city": "City Name",
  "province": "Province Name",
  "country": "Country Name",
  "phone": "+1234567890",
  "email": "contact@company.com",
  "logoUrl": "https://example.com/logo.png",
  "ivaTypeId": 1,
  "ivaType": "Responsable Inscripto",
  "grossIncome": "12345678901",
  "dateOfIncorporation": "2020-01-01T00:00:00Z"
}
```

### Get Fiscal Settings

```http
GET /api/CompanyInfo/fiscal-settings
```

Returns ARCA fiscal settings for invoice generation.

**Response:**
```json
{
  "taxId": "12345678901",
  "arcaEnabled": true,
  "arcaEnvironment": "Production",
  "arcaPointOfSale": 1,
  "arcaInvoiceTypesEnabled": "1,6"
}
```

### Update Company Information

```http
PUT /api/CompanyInfo
Authorization: Bearer {token}
```

Updates company information. Requires `Admin` role.

**Request Body:**
```json
{
  "name": "Updated Company Name",
  "shortName": "Updated Short Name",
  "taxId": "12345678901",
  "address": "Updated Address",
  "postalCodeId": 1,
  "ivaTypeId": 1,
  "phone": "+1234567890",
  "email": "contact@company.com",
  "logoUrl": "https://example.com/logo.png",
  "grossIncome": "12345678901",
  "dateOfIncorporation": "2020-01-01T00:00:00Z"
}
```

**Response:** `204 No Content`

### Update Logo URL

```http
PUT /api/CompanyInfo/logo-url
Authorization: Bearer {token}
```

Updates only the company logo URL. Requires `Admin` role.

**Request Body:**
```json
{
  "logoUrl": "https://example.com/new-logo.png"
}
```

**Response:** `204 No Content`

## Error Responses

The API uses standard HTTP status codes:

- `200 OK`: Request successful
- `204 No Content`: Update successful
- `400 Bad Request`: Validation failed or invalid data
- `401 Unauthorized`: JWT token missing or invalid
- `403 Forbidden`: User does not have required role
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

### Error Response Format

```json
{
  "message": "Error message",
  "errors": {
    "fieldName": ["Error message 1", "Error message 2"]
  }
}
```

## Database

### Entity Framework Migrations

Create a new migration after model changes:

```bash
dotnet ef migrations add MigrationName
```

Apply migrations:

```bash
dotnet ef database update
```

### Database Schema

The service uses a single table `CompanyInfo` with the following key fields:

- **Id**: Primary key
- **TaxId**: Unique tax identification number (indexed)
- **ArcaEnabled**: Safety switch for invoice generation
- **ArcaEnvironment**: Homologation or Production
- **ArcaPointOfSale**: Point of sale number (nullable)
- **ArcaInvoiceTypesEnabled**: Comma-separated invoice types

## Dependencies

- **Microsoft.EntityFrameworkCore.SqlServer**: Database access
- **Microsoft.AspNetCore.Authentication.JwtBearer**: JWT authentication
- **Swashbuckle.AspNetCore**: Swagger/OpenAPI documentation
- **AutoMapper.Extensions.Microsoft.DependencyInjection**: Object mapping (installed but not currently used)

## Development

### Running the Service

```bash
dotnet run
```

The service will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

### Swagger UI

In development mode, Swagger UI is available at the root URL (`/`).

### Testing

Use the provided `CompanyService.http` file for testing endpoints, or use tools like Postman or curl.

## Security

- **JWT Authentication**: All update endpoints require valid JWT tokens
- **Role-Based Authorization**: Update operations require `Admin` role
- **CORS**: Configured to allow requests from specified origins only
- **Input Validation**: All inputs are validated using DataAnnotations

## Integration

This microservice integrates with:

1. **Catalog Service**: Validates postal codes and IVA types
2. **Gateway Service**: Routes requests through API gateway
3. **Authentication Service**: Validates JWT tokens

## Deployment

### Azure App Service

1. Configure connection strings in Azure Portal
2. Set environment variables for sensitive configuration
3. Deploy using Azure DevOps or GitHub Actions
4. Configure CORS origins for production

### Docker (Optional)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "CompanyService.dll"]
```

## Logging

The service uses ASP.NET Core's built-in logging. Configure log levels in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## Contributing

1. Follow the existing code structure and patterns
2. Add XML documentation comments to public APIs
3. Ensure all validations are in place
4. Test endpoints before submitting changes

## License

[Your License Here]

## Support

For issues or questions, contact the development team.
