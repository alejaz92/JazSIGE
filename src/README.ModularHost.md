# JazSIGE Modular Host

Esta estructura propone un host modular en `src/Host` y módulos por dominio en `src/Modules/<Modulo>`.

## Convención por módulo
Cada módulo expone una clase `*ModuleInstaller` que implementa `IModuleInstaller` (`SharedKernel.Modularity`):

- `ConfigureServices`: registro DI del módulo.
- `MapEndpoints`: publicación de endpoints del módulo.
- `RoutePrefix` + `ApiVersion`: construcción de rutas versionadas.

## Versionado de API por prefijo
Se mantiene prefijo por contexto y versión:

- Catalog: `/api/catalog/v1/*`
- Sales: `/api/sales/v1/*`

De esta forma se mantiene estabilidad para frontend y permite nuevas versiones sin romper contratos existentes.
