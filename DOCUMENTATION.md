# 📋 Credit Card CRUD API

API REST completa para gestión de tarjetas de crédito implementada con **.NET 9**, **Clean Architecture**, **Domain-Driven Design (DDD)**, **Entity Framework Core**, **SQLite**, **Dapper** y preparada para **RabbitMQ**.

---

## 🏗️ **Arquitectura del Proyecto**

```
BankExamples/
🏗️ src/
│   🏗️ CreditCard.Domain/           # Capa de Dominio (DDD)
│   │   🏗️ Entities/                # Entidades con lógica de negocio
│   │   🏗️ Repositories/            # Interfaces de repositorios
│   │   🏗️ Events/                  # Eventos de dominio
│   │
│   🏗️ CreditCard.Application/      # Capa de Aplicación
│   │   🏗️ DTOs/                    # Data Transfer Objects
│   │   🏗️ Interfaces/              # Contratos de servicios
│   │   🏗️ Services/                # Lógica de aplicación + Reportes
│   │
│   🏗️ CreditCard.Infrastructure/   # Capa de Infraestructura
│   │   🏗️ Persistence/             # EF Core + UoW + Dapper
│   │   │   🏗️ Configurations/      # Configuraciones de entidades
│   │   │   🏗️ Repositories/        # Implementación de repositorios
│   │   │   🏗️ CreditCardDbContext.cs
│   │   🏗️ Messaging/               # RabbitMQ (preparado)
│   │
│   🏗️ CreditCard.Api/              # Capa de Presentación
│       🏗️ Endpoints/               # Minimal APIs
│       🏗️ Program.cs
```

**✅ Principios de Clean Architecture Respetados:**
- │ Domain no depende de nadie
- │ Application solo depende de Domain
- │ Infrastructure solo depende de Domain
- │ API depende de Application e Infrastructure (inyección de dependencias)
- │ **NO hay referencias circulares entre capas**

---

## 🛠️ **Tecnologóas Implementadas**

| Tecnologóa | Propósito |
|------------|-----------|
| **.NET 9** | Framework principal |
| **Entity Framework Core 9** | ORM para escrituras (CQRS Write) |
| **SQLite** | Base de datos ligera |
| **Dapper** | Consultas optimizadas para reportes (CQRS Read) |
| **Unit of Work** | Gestión de transacciones |
| **Repository Pattern** | Abstracción de acceso a datos |
| **DDD** | Lógica de negocio en el dominio |
| **Clean Architecture** | Separación de responsabilidades |
| **Minimal APIs** | Endpoints modernos y eficientes |

---

## ✅ **Funcionalidades Implementadas**

### **CRUD Completo**
- │ Crear tarjeta de crédito
- │ Obtener todas las tarjetas
- │ Obtener tarjeta por ID
- │ Actualizar tarjeta
- │ Eliminar tarjeta

### **Operaciones de Negocio**
- │ Realizar cargo a la tarjeta
- │ Realizar pago a la tarjeta
- │ Activar/Desactivar tarjeta

### **Reportes con Dapper**
- │ Reporte general de tarjetas con uso de crédito
- │ Reporte de tarjeta por ID
- │ Tarjetas activas
- │ Tarjetas con alto uso de crédito

---

## ✅ **Patrones Implementados**

### **Domain-Driven Design (DDD)**
```csharp
// Entidad con lógica de negocio
public class CreditCardEntity
{
    // Constructor privado para encapsulación
    private CreditCardEntity() { }

    // Factory Method
    public static CreditCardEntity Create(...) { }

    // Domain Methods con validaciones
    public void MakeCharge(decimal amount) { }
    public void MakePayment(decimal amount) { }
}
```

### **Unit of Work Pattern**
```csharp
public interface IUnitOfWork : IDisposable
{
    ICreditCardRepository CreditCards { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
}
```

### **CQRS Simplificado**
- **Escrituras**: EF Core + Repository Pattern
- **Lecturas**: Dapper con consultas optimizadas

---

## ✅ **Configuración y Ejecución**

### **1. Restaurar paquetes**
```bash
dotnet restore
```

### **2. Compilar el proyecto**
```bash
dotnet build
```

### **3. Ejecutar la aplicación**
```bash
cd src/CreditCard.Api
dotnet run
```

### **4. Acceder a Swagger**
```
https://localhost:5001/swagger
```

---

## ✅ **Endpoints Disponibles**

### **Gestión de Tarjetas**
| Mótodo | Endpoint | Descripción |
|--------|----------|-------------|
| `POST` | `/api/creditcards` | Crear nueva tarjeta |
| `GET` | `/api/creditcards` | Obtener todas las tarjetas |
| `GET` | `/api/creditcards/{id}` | Obtener tarjeta por ID |
| `PUT` | `/api/creditcards/{id}` | Actualizar tarjeta |
| `DELETE` | `/api/creditcards/{id}` | Eliminar tarjeta |

### **Operaciones**
| Mótodo | Endpoint | Descripción |
|--------|----------|-------------|
| `POST` | `/api/creditcards/{id}/charge` | Realizar cargo |
| `POST` | `/api/creditcards/{id}/payment` | Realizar pago |
| `POST` | `/api/creditcards/{id}/activate` | Activar tarjeta |
| `POST` | `/api/creditcards/{id}/deactivate` | Desactivar tarjeta |

### **Reportes (Dapper)**
| Mótodo | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/api/reports/creditcards` | Reporte completo |
| `GET` | `/api/reports/creditcards/{id}` | Reporte individual |
| `GET` | `/api/reports/creditcards/active` | Tarjetas activas |
| `GET` | `/api/reports/creditcards/high-usage│minPercentage=80` | Alto uso |

---

## ✅ **Ejemplos de Uso**

### **Crear Tarjeta**
```json
POST /api/creditcards
{
  "cardNumber": "4532015112830366",
  "cardHolderName": "Juan Pórez",
  "expirationDate": "12/2027",
  "cvv": "123",
  "creditLimit": 10000.00,
  "cardType": "Visa"
}
```

### **Realizar Cargo**
```json
POST /api/creditcards/{id}/charge
{
  "amount": 500.00
}
```

### **Realizar Pago**
```json
POST /api/creditcards/{id}/payment
{
  "amount": 300.00
}
```

### **Obtener Reporte de Alto Uso**
```http
GET /api/reports/creditcards/high-usage│minPercentage=70
```

---

## ✅ **Integración con RabbitMQ**

El proyecto está preparado para integración con RabbitMQ:

```csharp
// Interface disponible
public interface IMessagePublisher
{
    Task PublishAsync<T>(T message, string routingKey);
}

// Uso en servicios
await _messagePublisher.PublishAsync(
    new CreditCardCreatedEvent(card.Id, card.CardNumber),
    "creditcard.created"
);
```

Para habilitar RabbitMQ real:
1. Instalar `RabbitMQ.Client`
2. Implementar `RabbitMqPublisher` con conexión real
3. Configurar en `appsettings.json`

---

## ✅ **Reglas de Negocio Implementadas**

- │ Nómero de tarjeta ónico
- │ Validación de nómero de tarjeta (13-19 dógitos)
- │ CVV de 3-4 dógitos
- │ No permitir cargos mayores al crédito disponible
- │ No permitir operaciones en tarjetas inactivas
- │ Cólculo automótico de crédito disponible
- │ Validación de pagos que excedan el lómite

---

## ✅ **Base de Datos**

### **Esquema SQLite**
```sql
CREATE TABLE CreditCards (
    Id TEXT PRIMARY KEY,
    CardNumber TEXT NOT NULL UNIQUE,
    CardHolderName TEXT NOT NULL,
    ExpirationDate TEXT NOT NULL,
    CVV TEXT NOT NULL,
    CreditLimit REAL NOT NULL,
    AvailableCredit REAL NOT NULL,
    CardType TEXT NOT NULL,
    IsActive INTEGER NOT NULL,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT
);
```

---

## ✅ **Pruebas**

### **Probar con cURL**
```bash
# Crear tarjeta
curl -X POST https://localhost:5001/api/creditcards \
  -H "Content-Type: application/json" \
  -d '{
    "cardNumber": "4532015112830366",
    "cardHolderName": "Juan Pórez",
    "expirationDate": "12/2027",
    "cvv": "123",
    "creditLimit": 10000,
    "cardType": "Visa"
  }'
```

---

## ✅ **Paquetes NuGet Utilizados**

### **Infrastructure**
- `Microsoft.EntityFrameworkCore` 9.0.0
- `Microsoft.EntityFrameworkCore.Sqlite` 9.0.0
- `Microsoft.EntityFrameworkCore.Design` 9.0.0
- `Dapper` 2.1.35
- `Microsoft.Data.Sqlite` 9.0.0

### **Application**
- `Microsoft.Extensions.DependencyInjection.Abstractions` 9.0.0

### **Api**
- `Microsoft.AspNetCore.OpenApi` 9.0.11
- `Swashbuckle.AspNetCore` 7.2.0

---

## ✅ **Próximos Pasos (Extensiones Sugeridas)**

1. **Autenticación y Autorización**: JWT Tokens
2. **Logging**: Serilog con Seq
3. **Validación**: FluentValidation
4. **Mapeo**: AutoMapper
5. **Testing**: xUnit + Moq
6. **Cache**: Redis
7. **MediatR**: Para CQRS completo
8. **Health Checks**: Para monitoreo
9. **Rate Limiting**: Protección API
10. **Docker**: Containerización

---

## 🏗️✅ **Desarrollado con**

- Clean Architecture
- Domain-Driven Design
- SOLID Principles
- Repository Pattern
- Unit of Work Pattern
- CQRS (Simplificado)
- Dependency Injection

---

óDisfruta del proyecto! 📋
