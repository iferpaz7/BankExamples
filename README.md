# Credit Card API

API REST para gestión de tarjetas de crédito desarrollada con .NET 9 y Clean Architecture.

## 🏗️ Arquitectura

El proyecto sigue los principios de **Clean Architecture** con las siguientes capas:

```
src/
├── CreditCard.Api/           # Capa de presentación (Minimal APIs)
├── CreditCard.Application/   # Capa de aplicación (Servicios, DTOs)
├── CreditCard.Domain/        # Capa de dominio (Entidades, Repositorios)
└── CreditCard.Infrastructure/# Capa de infraestructura (EF Core, Persistencia)

tests/
└── CreditCard.Tests/         # Pruebas unitarias
```

## 📋 Requisitos

- .NET 9 SDK
- SQL Server (o SQL Server LocalDB)

## ⚙️ Configuración

1. Clonar el repositorio
2. Restaurar paquetes: `dotnet restore`
3. Ejecutar migraciones: Las migraciones se ejecutan automáticamente en modo desarrollo
4. Ejecutar la aplicación: `dotnet run --project src/CreditCard.Api`

## 🚀 API Endpoints

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/creditcards` | Crear tarjeta de crédito |
| GET | `/api/creditcards` | Obtener todas las tarjetas |
| GET | `/api/creditcards/{id}` | Obtener tarjeta por ID |
| PUT | `/api/creditcards/{id}` | Actualizar tarjeta |
| DELETE | `/api/creditcards/{id}` | Eliminar tarjeta |
| POST | `/api/creditcards/{id}/charge` | Realizar cargo |
| POST | `/api/creditcards/{id}/payment` | Realizar pago |
| POST | `/api/creditcards/{id}/deactivate` | Desactivar tarjeta |
| POST | `/api/creditcards/{id}/activate` | Activar tarjeta |

## 🧪 Pruebas Unitarias

El proyecto incluye pruebas unitarias exhaustivas usando **xUnit**, **Moq** y **FluentAssertions**.

### Ejecutar Pruebas

```bash
dotnet test
```

### Ejecutar Pruebas con Cobertura

```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## 📊 Catálogo de Pruebas

### 🏛️ Pruebas de Dominio - `CreditCardEntity`

#### ✅ Pruebas Positivas

| Prueba | Descripción | Resultado Esperado |
|--------|-------------|-------------------|
| `Create_ConDatosValidos_DebeCrearTarjeta` | Crear tarjeta con todos los datos válidos | Tarjeta creada con ID, estado activo y crédito disponible igual al límite |
| `Create_ConNumeroTarjetaValido` | Crear tarjeta con número de 13-19 dígitos | Tarjeta creada correctamente |
| `Create_ConCVVValido` | Crear tarjeta con CVV de 3-4 dígitos | Tarjeta creada correctamente |
| `UpdateCardHolder_ConNombreValido_DebeActualizarNombre` | Actualizar nombre del titular | Nombre actualizado en mayúsculas, UpdatedAt establecido |
| `UpdateCreditLimit_ConLimiteMayor_DebeAumentarCreditoDisponible` | Aumentar límite de crédito | Límite y crédito disponible incrementados |
| `UpdateCreditLimit_ConLimiteMenor_DebeReducirCreditoDisponible` | Reducir límite de crédito | Límite y crédito disponible reducidos |
| `MakeCharge_ConMontoValido_DebeReducirCreditoDisponible` | Realizar cargo válido | Crédito disponible reducido por el monto del cargo |
| `MakeCharge_ConMontoIgualACreditoDisponible_DebeDejarCreditoEnCero` | Cargo por el total disponible | Crédito disponible en cero |
| `MakePayment_ConMontoValido_DebeAumentarCreditoDisponible` | Realizar pago válido | Crédito disponible aumentado |
| `MakePayment_ConPagoTotal_DebeRestaurarCreditoCompleto` | Pagar deuda completa | Crédito disponible igual al límite |
| `Deactivate_DebeDesactivarTarjeta` | Desactivar tarjeta | IsActive = false |
| `Activate_DebeActivarTarjeta` | Activar tarjeta desactivada | IsActive = true |

#### ❌ Pruebas Negativas

| Prueba | Descripción | Excepción Esperada |
|--------|-------------|-------------------|
| `Create_ConNumeroTarjetaNuloOVacio_DebeLanzarExcepcion` | Número de tarjeta nulo, vacío o espacios | `ArgumentException: "El número de tarjeta es requerido"` |
| `Create_ConNumeroTarjetaLongitudInvalida_DebeLanzarExcepcion` | Número < 13 o > 19 dígitos | `ArgumentException: "El número de tarjeta debe tener entre 13 y 19 dígitos"` |
| `Create_ConNombreTitularNuloOVacio_DebeLanzarExcepcion` | Nombre nulo, vacío o espacios | `ArgumentException: "El nombre del titular es requerido"` |
| `Create_ConNombreTitularMuyCorto_DebeLanzarExcepcion` | Nombre < 3 caracteres | `ArgumentException: "El nombre debe tener al menos 3 caracteres"` |
| `Create_ConCVVNuloOVacio_DebeLanzarExcepcion` | CVV nulo, vacío o espacios | `ArgumentException: "El CVV es requerido"` |
| `Create_ConCVVLongitudInvalida_DebeLanzarExcepcion` | CVV < 3 o > 4 dígitos | `ArgumentException: "El CVV debe tener 3 o 4 dígitos"` |
| `UpdateCardHolder_ConNombreNuloOVacio_DebeLanzarExcepcion` | Actualizar con nombre inválido | `ArgumentException: "El nombre del titular es requerido"` |
| `UpdateCardHolder_ConNombreMuyCorto_DebeLanzarExcepcion` | Nombre < 3 caracteres | `ArgumentException: "El nombre debe tener al menos 3 caracteres"` |
| `UpdateCreditLimit_ConLimiteCeroONegativo_DebeLanzarExcepcion` | Límite <= 0 | `ArgumentException: "El límite de crédito debe ser mayor a 0"` |
| `MakeCharge_ConMontoCeroONegativo_DebeLanzarExcepcion` | Monto <= 0 | `ArgumentException: "El monto debe ser mayor a 0"` |
| `MakeCharge_ConTarjetaInactiva_DebeLanzarExcepcion` | Cargo en tarjeta desactivada | `InvalidOperationException: "La tarjeta está inactiva"` |
| `MakeCharge_ConMontoMayorACreditoDisponible_DebeLanzarExcepcion` | Monto > crédito disponible | `InvalidOperationException: "Crédito insuficiente"` |
| `MakePayment_ConMontoCeroONegativo_DebeLanzarExcepcion` | Monto <= 0 | `ArgumentException: "El monto debe ser mayor a 0"` |
| `MakePayment_ConPagoQueExcedeLimite_DebeLanzarExcepcion` | Pago que excede límite de crédito | `InvalidOperationException: "El pago excede el límite de crédito"` |

---

### 💼 Pruebas de Aplicación - `CreditCardService`

#### ✅ Pruebas Positivas

| Prueba | Descripción | Resultado Esperado |
|--------|-------------|-------------------|
| `CreateAsync_ConDatosValidos_DebeCrearTarjetaYRetornarDto` | Crear tarjeta nueva | DTO con datos de la tarjeta creada, repositorio y UoW invocados |
| `GetByIdAsync_ConIdExistente_DebeRetornarDto` | Obtener tarjeta existente | DTO con datos de la tarjeta |
| `GetAllAsync_ConTarjetasExistentes_DebeRetornarListaDeDtos` | Obtener todas las tarjetas | Lista de DTOs |
| `GetAllAsync_SinTarjetas_DebeRetornarListaVacia` | Sin tarjetas en BD | Lista vacía |
| `UpdateAsync_ConDatosValidos_DebeActualizarYRetornarDto` | Actualizar tarjeta | DTO con datos actualizados |
| `DeleteAsync_ConIdExistente_DebeEliminarTarjeta` | Eliminar tarjeta | Repositorio.Delete invocado |
| `MakeChargeAsync_ConMontoValido_DebeReducirCreditoYRetornarDto` | Realizar cargo | Crédito reducido en el DTO |
| `MakePaymentAsync_ConMontoValido_DebeAumentarCreditoYRetornarDto` | Realizar pago | Crédito aumentado en el DTO |
| `DeactivateAsync_ConIdExistente_DebeDesactivarYRetornarDto` | Desactivar tarjeta | IsActive = false en DTO |
| `ActivateAsync_ConIdExistente_DebeActivarYRetornarDto` | Activar tarjeta | IsActive = true en DTO |

#### ❌ Pruebas Negativas

| Prueba | Descripción | Excepción Esperada |
|--------|-------------|-------------------|
| `CreateAsync_ConNumeroTarjetaExistente_DebeLanzarExcepcion` | Número de tarjeta duplicado | `InvalidOperationException: "Ya existe una tarjeta con este número"` |
| `GetByIdAsync_ConIdInexistente_DebeRetornarNull` | ID no existe | `null` |
| `UpdateAsync_ConIdInexistente_DebeLanzarExcepcion` | Actualizar tarjeta inexistente | `InvalidOperationException: "Tarjeta no encontrada"` |
| `DeleteAsync_ConIdInexistente_DebeLanzarExcepcion` | Eliminar tarjeta inexistente | `InvalidOperationException: "Tarjeta no encontrada"` |
| `MakeChargeAsync_ConIdInexistente_DebeLanzarExcepcion` | Cargo en tarjeta inexistente | `InvalidOperationException: "Tarjeta no encontrada"` |
| `MakeChargeAsync_ConCreditoInsuficiente_DebeLanzarExcepcion` | Monto > crédito disponible | `InvalidOperationException: "Crédito insuficiente"` |
| `MakePaymentAsync_ConIdInexistente_DebeLanzarExcepcion` | Pago en tarjeta inexistente | `InvalidOperationException: "Tarjeta no encontrada"` |
| `MakePaymentAsync_ConPagoQueExcedeLimite_DebeLanzarExcepcion` | Pago excede límite | `InvalidOperationException: "El pago excede el límite de crédito"` |
| `DeactivateAsync_ConIdInexistente_DebeLanzarExcepcion` | Desactivar tarjeta inexistente | `InvalidOperationException: "Tarjeta no encontrada"` |
| `ActivateAsync_ConIdInexistente_DebeLanzarExcepcion` | Activar tarjeta inexistente | `InvalidOperationException: "Tarjeta no encontrada"` |

---

### 📈 Pruebas de Aplicación - `CreditCardReportService`

#### ✅ Pruebas Positivas

| Prueba | Descripción | Resultado Esperado |
|--------|-------------|-------------------|
| `GetCreditCardsReportAsync_ConTarjetasExistentes_DebeRetornarListaDeReportes` | Obtener reporte de tarjetas | Lista de reportes DTO |
| `GetCreditCardsReportAsync_SinTarjetas_DebeRetornarListaVacia` | Sin tarjetas en BD | Lista vacía |
| `GetCreditCardReportByIdAsync_ConIdExistente_DebeRetornarReporte` | Obtener reporte por ID | Reporte DTO con datos |
| `GetActiveCreditCardsAsync_ConTarjetasActivas_DebeRetornarSoloActivas` | Obtener tarjetas activas | Lista de tarjetas activas |
| `GetActiveCreditCardsAsync_SinTarjetasActivas_DebeRetornarListaVacia` | Sin tarjetas activas | Lista vacía |
| `GetCreditCardsWithHighUsageAsync_ConTarjetasAltoUso_DebeRetornarFiltradas` | Tarjetas con alto uso | Lista filtrada |
| `GetCreditCardsWithHighUsageAsync_SinTarjetasAltoUso_DebeRetornarListaVacia` | Sin tarjetas alto uso | Lista vacía |
| `GetCreditCardsWithHighUsageAsync_ConPorcentajeCero_DebeRetornarTodas` | Porcentaje 0% | Todas las tarjetas |
| `GetCreditCardsWithHighUsageAsync_ConPorcentaje100_DebeRetornarSoloMaximoUso` | Porcentaje 100% | Solo uso máximo |

#### ❌ Pruebas Negativas

| Prueba | Descripción | Resultado Esperado |
|--------|-------------|-------------------|
| `GetCreditCardReportByIdAsync_ConIdInexistente_DebeRetornarNull` | ID no existe | `null` |

---

### 🔬 Pruebas de Casos Edge - `CreditCardEntity`

| Prueba | Descripción | Resultado Esperado |
|--------|-------------|-------------------|
| `Deactivate_TarjetaYaInactiva_DebePermaneceInactiva` | Desactivar tarjeta ya inactiva | Permanece inactiva |
| `Activate_TarjetaYaActiva_DebePermaneceActiva` | Activar tarjeta ya activa | Permanece activa |
| `MakeCharge_MultiplesCargos_DebeAcumularCorrectamente` | Múltiples cargos | Acumulación correcta |
| `MakePayment_MultiplesPagos_DebeAcumularCorrectamente` | Múltiples pagos | Acumulación correcta |
| `CicloCompleto_CargosYPagos_DebeCalcularCorrectamente` | Ciclo cargos/pagos | Cálculo correcto |
| `UpdateCreditLimit_ConDeudaExistente_DebeAjustarCreditoDisponible` | Cambio límite con deuda | Ajuste correcto |
| `Create_ConNombreEnMinusculas_DebeConvertirAMayusculas` | Nombre minúsculas | Convertido a mayúsculas |
| `Create_ConNombreMixto_DebeConvertirAMayusculas` | Nombre mixto | Convertido a mayúsculas |
| `Create_ConDiferentesTiposTarjeta` | Tipos Visa/MC/Amex/Discover | Creación correcta |
| `Create_ConDiferentesLimites` | Límites 0.01/1/999999.99 | Creación correcta |

---

## 🔗 Pruebas de Integración

Las pruebas de integración validan la interacción entre componentes reales del sistema.

### Ejecutar Pruebas de Integración

```bash
dotnet test tests/CreditCard.IntegrationTests
```

### 🌐 Pruebas de API Endpoints

| Prueba | Método | Endpoint | Resultado Esperado |
|--------|--------|----------|-------------------|
| `CreateCreditCard_ConDatosValidos` | POST | `/api/creditcards` | 201 Created |
| `CreateCreditCard_ConNumeroTarjetaDuplicado` | POST | `/api/creditcards` | 400 BadRequest |
| `CreateCreditCard_ConDatosInvalidos` | POST | `/api/creditcards` | 400 BadRequest |
| `GetAllCreditCards` | GET | `/api/creditcards` | 200 OK |
| `GetCreditCardById_ConIdExistente` | GET | `/api/creditcards/{id}` | 200 OK |
| `GetCreditCardById_ConIdInexistente` | GET | `/api/creditcards/{id}` | 404 NotFound |
| `UpdateCreditCard_ConDatosValidos` | PUT | `/api/creditcards/{id}` | 200 OK |
| `UpdateCreditCard_ConIdInexistente` | PUT | `/api/creditcards/{id}` | 400 BadRequest |
| `DeleteCreditCard_ConIdExistente` | DELETE | `/api/creditcards/{id}` | 204 NoContent |
| `DeleteCreditCard_ConIdInexistente` | DELETE | `/api/creditcards/{id}` | 400 BadRequest |
| `MakeCharge_ConMontoValido` | POST | `/api/creditcards/{id}/charge` | 200 OK |
| `MakeCharge_ConCreditoInsuficiente` | POST | `/api/creditcards/{id}/charge` | 400 BadRequest |
| `MakePayment_ConMontoValido` | POST | `/api/creditcards/{id}/payment` | 200 OK |
| `MakePayment_ConPagoQueExcedeLimite` | POST | `/api/creditcards/{id}/payment` | 400 BadRequest |
| `DeactivateCreditCard_ConIdExistente` | POST | `/api/creditcards/{id}/deactivate` | 200 OK |
| `ActivateCreditCard_ConTarjetaDesactivada` | POST | `/api/creditcards/{id}/activate` | 200 OK |
| `FlujoCompleto_CRUD` | Múltiples | Múltiples | Flujo exitoso |
| `FlujoCompleto_MultiplesCargosYPagos` | Múltiples | Múltiples | Cálculo correcto |

### 💾 Pruebas de Repository

| Prueba | Descripción | Resultado Esperado |
|--------|-------------|-------------------|
| `AddAsync_ConEntidadValida` | Agregar tarjeta a BD | Entidad persistida |
| `AddAsync_MultiplesTarjetas` | Agregar múltiples tarjetas | Todas persistidas |
| `GetByIdAsync_ConIdExistente` | Obtener por ID existente | Entidad retornada |
| `GetByIdAsync_ConIdInexistente` | Obtener por ID inexistente | null |
| `GetAllAsync_ConTarjetasExistentes` | Obtener todas | Lista completa |
| `GetAllAsync_SinTarjetas` | Obtener sin datos | Lista vacía |
| `GetAllAsync_OrdenadoPorFecha` | Orden de resultados | Ordenado DESC |
| `GetByCardNumberAsync_ConNumeroExistente` | Buscar por número | Entidad retornada |
| `GetByCardNumberAsync_ConNumeroInexistente` | Buscar número inexistente | null |
| `Update_ConCambiosValidos` | Actualizar entidad | Cambios persistidos |
| `Update_ConCargo` | Actualizar con cargo | Crédito reducido |
| `Update_ConPago` | Actualizar con pago | Crédito aumentado |
| `Delete_ConEntidadExistente` | Eliminar entidad | Entidad eliminada |
| `MultipleOperaciones_Concurrentes` | Operaciones concurrentes | Integridad mantenida |

### 🔄 Pruebas de UnitOfWork

| Prueba | Descripción | Resultado Esperado |
|--------|-------------|-------------------|
| `SaveChangesAsync_ConCambiosPendientes` | Guardar cambios | Cambios persistidos |
| `SaveChangesAsync_SinCambios` | Guardar sin cambios | Retorna 0 |
| `CreditCards_DebeRetornarRepositorioFuncional` | Acceso a repositorio | Repositorio funcional |
| `CreditCards_MultiplesAccesos_MismaInstancia` | Singleton de repositorio | Misma instancia |
| `FlujoCompleto_CrearActualizarEliminar` | Flujo CRUD con UoW | Operaciones exitosas |
| `FlujoCompleto_CargosYPagos` | Cargos y pagos con UoW | Crédito actualizado |
| `FlujoCompleto_ActivarDesactivar` | Activar/Desactivar con UoW | Estado actualizado |
| `MultiplesOperaciones_EnUnaSolaTransaccion` | Múltiples operaciones | Todas guardadas |

---

## 📑 Resumen de Pruebas

| Categoría | Positivas | Negativas | Total |
|-----------|-----------|-----------|-------|
| **PRUEBAS UNITARIAS** | | | |
| Domain (CreditCardEntity) | 22 | 14 | 36 |
| Application (CreditCardService) | 10 | 10 | 20 |
| Application (CreditCardReportService) | 9 | 1 | 10 |
| Casos Edge | 24 | 0 | 24 |
| **Subtotal Unitarias** | **65** | **25** | **90** |
| **PRUEBAS DE INTEGRACIÓN** | | | |
| API Endpoints | 16 | 6 | 22 |
| Repository | 12 | 2 | 14 |
| UnitOfWork | 8 | 0 | 8 |
| **Subtotal Integración** | **36** | **8** | **44** |
| **TOTAL GENERAL** | **101** | **33** | **134** |

## 📦 Archivos de Entrega

Los archivos para el pase de producción se encuentran en:

| Archivo | Ubicación | Descripción |
|---------|-----------|-------------|
| Pruebas Unitarias + Integración | `tests/CreditCard.Tests/TestResults/PruebasUnitarias_CreditCard.csv` | CSV con 134 pruebas |

## 🛠️ Tecnologías Utilizadas

- **.NET 9** - Framework principal
- **Entity Framework Core 9** - ORM
- **SQL Server** - Base de datos
- **xUnit** - Framework de pruebas
- **Moq** - Framework de mocking
- **FluentAssertions** - Librería de aserciones fluidas
- **Swagger/OpenAPI** - Documentación de API
- **NBomber** - Framework de pruebas de carga

---

## 🚀 Pruebas de Carga

El proyecto incluye pruebas de carga masiva utilizando **NBomber** para evaluar el rendimiento de la API.

### Estructura

```
tests/
└── CreditCard.LoadTests/     # Proyecto de pruebas de carga (consola)
src/
└── CreditCard.Web/           # Interfaz Blazor con panel de pruebas de carga
```

### Tipos de Pruebas de Carga

| Tipo | Descripción | Configuración Por Defecto |
|------|-------------|---------------------------|
| **Smoke Test** | Validación básica con carga mínima | 30s, 5 RPS |
| **Load Test** | Simulación de carga normal esperada | 60s, 20 RPS |
| **Stress Test** | Encontrar límites del sistema | 60s, 50 RPS |
| **Spike Test** | Simular picos repentinos de tráfico | 60s, 100 RPS |
| **CRUD Flow Test** | Flujo completo de operaciones CRUD | 120s, 5 RPS |
| **Reports Test** | Pruebas de endpoints de reportes | 120s, 10 RPS |

### Ejecutar Pruebas de Carga (Consola)

```bash
# Ejecutar el proyecto de pruebas de carga
dotnet run --project tests/CreditCard.LoadTests

# Seleccionar opción del menú:
# 1. Smoke Test
# 2. Load Test
# 3. Stress Test
# 4. Spike Test
# 5. CRUD Flow Test
# 6. Reports Test
# 7. Ejecutar TODAS
```

### Ejecutar Pruebas de Carga (Web - Blazor)

1. Iniciar la API:
```bash
dotnet run --project src/CreditCard.Api
```

2. Iniciar la aplicación web:
```bash
dotnet run --project src/CreditCard.Web
```

3. Navegar a `https://localhost:xxxx/loadtests`

4. Configurar y ejecutar las pruebas desde la interfaz gráfica

### Métricas Capturadas

| Métrica | Descripción |
|---------|-------------|
| **Total Requests** | Número total de peticiones realizadas |
| **Successful Requests** | Peticiones exitosas (2xx) |
| **Failed Requests** | Peticiones fallidas |
| **RPS (Requests/Second)** | Throughput promedio |
| **Average Response Time** | Tiempo de respuesta promedio |
| **P50 (Mediana)** | Percentil 50 de tiempo de respuesta |
| **P95** | Percentil 95 de tiempo de respuesta |
| **P99** | Percentil 99 de tiempo de respuesta |
| **Error Rate** | Porcentaje de errores |

### Criterios de Éxito

- ✅ Tasa de error < 5%
- ✅ P95 < 1000ms

### Reportes

Los reportes de NBomber se generan automáticamente en:
- `tests/CreditCard.LoadTests/LoadTestResults/`
- Formatos: HTML, CSV, Markdown

### Configuración

Modificar `tests/CreditCard.LoadTests/appsettings.json`:

```json
{
  "LoadTest": {
    "BaseUrl": "https://localhost:7001",
    "Scenarios": {
      "SmokeTest": {
        "Duration": 60,
        "UsersPerSecond": 5
      },
      "LoadTest": {
        "Duration": 300,
        "UsersPerSecond": 50
      }
    }
  }
}
```

---

## 📄 Licencia

Este proyecto está bajo la licencia MIT.
