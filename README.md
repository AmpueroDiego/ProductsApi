# Products API

REST API en **.NET 10** para una evaluación técnica de backend. Permite crear,
actualizar y consultar productos. Al consultar un producto, compone la respuesta
con el nombre del estado (desde un cache en memoria) y un descuento traído de un
servicio externo, calculando el precio final.

## Qué hace

- **POST `/api/products`**: crea un producto. Devuelve `201 Created` con header
  `Location`, `400` si el request es inválido y `409 Conflict` si el `ProductId`
  ya existe.
- **PUT `/api/products/{productId}`**: actualiza un producto. Devuelve `200 OK`
  con el producto actualizado, `404` si no existe y `400` si es inválido.
- **GET `/api/products/{productId}`**: devuelve `ProductId`, `Name`,
  `StatusName`, `Stock`, `Description`, `Price`, `Discount` y `FinalPrice`.
  `404` si no existe.
  - `StatusName` viene del cache, no de la base de datos.
  - `Discount` es un porcentaje (0-100) traído del servicio externo por `ProductId`.
  - `FinalPrice` = `Price * (100 - Discount) / 100`, redondeado a 2 decimales.

## Arquitectura: las 4 capas

El proyecto separa responsabilidades en 4 capas + un proyecto de tests. La regla
central es **inversión de dependencias**: `Application` define las interfaces e
`Infrastructure` las implementa. `Application` nunca referencia Dapper, SqlClient
ni HttpClient.

- **`Products.Domain`**: la entidad `Product`. Sin dependencias de nada.
- **`Products.Application`**: la lógica de negocio (`ProductService`), los DTOs
  (con validación por Data Annotations) y las **interfaces** (`IProductRepository`,
  `IDiscountService`, `IStatusCache`). Depende solo de `Domain`.
- **`Products.Infrastructure`**: las implementaciones concretas —
  `ProductRepository` (Dapper), `StatusCache` (IMemoryCache) y `DiscountService`
  (HttpClient). Implementa las interfaces de `Application`.
- **`Products.Api`**: los controllers, el middleware de tiempos, `Program.cs` y el
  registro de dependencias (DI).
- **`Products.Tests`**: un único proyecto de tests (xUnit + FluentAssertions +
  NSubstitute).

Dirección de dependencias: `Api` → `Application` + `Infrastructure`;
`Infrastructure` → `Application`; `Application` → `Domain`.

## Patrones usados

- **Repository** (`IProductRepository` / `ProductRepository`): aísla el acceso a
  datos detrás de una interfaz. El resto del código trabaja con productos sin
  saber que por debajo hay Dapper y SQL Server.
- **Dependency Injection**: las dependencias se inyectan por constructor y se
  registran en `Program.cs`. Esto permite cambiar implementaciones y mockearlas en
  los tests sin tocar la lógica.
- **Middleware**: `RequestTimingMiddleware` mide el tiempo de cada request. El
  manejo global de errores se resuelve con `UseExceptionHandler`, sin ensuciar los
  controllers.

## Por qué Dapper y no EF Core

- **Control sobre el SQL**: las queries se escriben a mano, exactamente como se
  ejecutan. No hay traducción mágica de LINQ.
- **Menos abstracción**: Dapper es un micro-ORM; mapea filas a objetos y poco más.
  No hay change tracking, contexto ni migraciones que entender.
- **Rendimiento**: al ser una capa delgada sobre ADO.NET, tiene muy poco overhead.

Todas las queries usan **parámetros** (`@ProductId`, etc.), nunca concatenación de
strings, para evitar inyección SQL.

## Cómo levantarlo

Requisitos: .NET 10 SDK y una instancia de SQL Server (Express, Developer o
LocalDB) con autenticación de Windows.

1. **Clonar** el repositorio.
2. **Crear la base de datos**: correr `db/schema.sql` en SSMS, o por consola
   (reemplazá el `-S` por tu servidor):
   ```powershell
   sqlcmd -S "DESKTOP-N7JPT0O" -i db/schema.sql
   ```
3. **Ajustar la connection string** en
   `src/Products.Api/appsettings.json`, sección `ConnectionStrings:SqlServer`.
   Cambiá `Server=` por el nombre de tu instancia (podés usar `localhost` o `.`):
   ```
   Server=DESKTOP-N7JPT0O;Database=ProductsDb;Trusted_Connection=True;TrustServerCertificate=True;
   ```
4. **Configurar el servicio de descuento** (ver sección más abajo) y poner la URL
   base en `DiscountService:BaseUrl`.
5. **Restaurar y correr**:
   ```powershell
   dotnet restore
   dotnet run --project src/Products.Api
   ```

### Swagger

Con el proyecto corriendo en modo Development, la UI de Swagger está en:

- `http://localhost:5072/swagger`
- `https://localhost:7204/swagger`

## Cómo correr los tests

```powershell
dotnet test
```

Los tests no tocan SQL Server ni la red: todo está mockeado con NSubstitute.

## Configurar el mock en mockapi.io

El `DiscountService` consulta un endpoint externo por `ProductId`. Para crear el
mock:

1. Entrá a [mockapi.io](https://mockapi.io) y creá un proyecto nuevo. Te da una URL
   base del tipo `https://<id>.mockapi.io/api/v1/`.
2. Creá un recurso llamado **`discounts`** con estos campos:
   - `productId` (Number)
   - `discount` (Number, entre 0 y 100)
3. Agregá algunos registros de ejemplo, por ejemplo:
   ```json
   { "productId": 1, "discount": 20 }
   ```
4. Copiá la URL base (incluyendo la barra final) en `appsettings.json`:
   ```json
   "DiscountService": {
     "BaseUrl": "https://<id>.mockapi.io/api/v1/"
   }
   ```

El servicio hace `GET {BaseUrl}discounts/{productId}` y espera una respuesta con la
forma `{ "productId": 1, "discount": 20 }`. Si el servicio falla, da timeout o
devuelve algo inesperado, el `DiscountService` devuelve `Discount = 0` en lugar de
tumbar el request.

## Cache de estados

El diccionario `{ 1: "Active", 0: "Inactive" }` se guarda en `IMemoryCache` con
expiración absoluta. El TTL se lee de `appsettings.json`
(`StatusCache:TtlMinutes`, por defecto 5), no está hardcodeado.

## Log de tiempo de respuesta

`RequestTimingMiddleware` mide cada request con `Stopwatch` y escribe una línea en
`logs/requests.txt` (con `try/finally`, así registra aunque el request termine en
excepción). Formato:

```
2026-07-22 14:30:01 | GET /api/products/1 | 200 | 45ms
```

## Manejo de errores

`UseExceptionHandler` (en `Program.cs`) centraliza los errores y devuelve un JSON
consistente. Los controllers no tienen `try/catch`. Mapea:

- `ProductNotFoundException` → `404`
- `ProductAlreadyExistsException` → `409`
- cualquier otra excepción → `500`

## Decisiones y supuestos

- **`PriceDiscount` vs `Discount`**: el enunciado nombra el campo `PriceDiscount`
  en un lado y usa `Discount` en la fórmula. Se devuelven `Price`, `Discount` y
  `FinalPrice` por separado en la respuesta del GET. `Discount` es el porcentaje
  del servicio externo; no se persiste en la tabla `Products`.
- **Redondeo de `FinalPrice`**: se usa el redondeo clásico de .NET
  (`Math.Round(value, 2)`, banker's rounding / `ToEven`).
- **PUT devuelve la misma composición que GET** (`StatusName`, `Discount`,
  `FinalPrice`) para mantener la respuesta consistente entre endpoints.
- **`IDbConnectionFactory`**: se registra como singleton, pero cada llamada a
  `CreateConnection()` devuelve una `SqlConnection` nueva. No se comparte una
  conexión entre requests.
- **Validación con Data Annotations**: los DTOs se anotan con `[Required]`,
  `[MaxLength]` y `[Range]`. Gracias a `[ApiController]`, ASP.NET valida
  automáticamente y devuelve `400` con el detalle por campo
  (`ValidationProblemDetails`) antes de entrar al controller.
- **Repo git propio**: este proyecto tiene su propio repositorio git local. No se
  hizo push a ningún remoto.

## Paquetes principales

- `Dapper`, `Microsoft.Data.SqlClient`
- `Swashbuckle.AspNetCore` (Swagger UI)
- `Microsoft.Extensions.Caching.Memory` (IMemoryCache)
- Tests: `xUnit`, `FluentAssertions` 7.2.0, `NSubstitute`
