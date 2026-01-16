# 🏗️ Arquitectura Clean Architecture - Diagrama de Dependencias

## Estructura de Capas y Flujo de Dependencias

```
┌────────────────────────────────────────────────────────────┐
│                    CreditCard.Api                          │
│                (Capa de Presentación)                      │
│                                                            │
│  ├─ Program.cs                                             │
│  ├─ Endpoints/CreditCardEndpoints.cs                      │
│  └─ Endpoints/ReportEndpoints.cs                          │
│                                                            │
│  Depende de: ─→ Application + Infrastructure              │
└─────────────────┬────────────────────────────────────────┘
                  │
        ┌─────────┴─────────┐
        │                   │
   ┌────▼──────────┐   ┌────▼──────────────────┐
   │  CreditCard   │   │    CreditCard         │
   │ Application   │   │  Infrastructure       │
   │ (Capa de      │   │  (Capa de             │
   │ Aplicación)   │   │  Infraestructura)     │
   │               │   │                       │
   │ ├─ DTOs/      │   │ ├─ Persistence/       │
   │ ├─ Interfaces │   │   - DbContext         │
   │ └─ Services/  │   │   - Repositories/     │
   │   - Credit    │   │     * CreditCardRepo  │
   │     CardService   │     * UnitOfWork      │
   │   - Credit    │   │     * ReadRepository  │
   │     CardReport    │       (Dapper)        │
   │     Service   │   │ ├─ Messaging/         │
   │               │   │   - RabbitMqPublisher │
   │ Depende de:   │   │                       │
   │  ─→ Domain    │   │ Depende de: ─→ Domain│
   └────┬──────────┘   └───────┬────────────────┘
        │                      │
        └──────────┬───────────┘
                   │
           ┌───────▼────────┐
           │ CreditCard.    │
           │    Domain      │
           │ (Capa de       │
           │  Dominio)      │
           │                │
           │ ├─ Entities/   │
           │   - CreditCard │
           │     Entity     │
           │ ├─ Repositories│
           │   - ICredit    │
           │     CardRepo   │
           │   - ICredit    │
           │     CardRead   │
           │     Repository │
           │   - IUnitOfWork│
           │ ├─ Events/     │
           │   - IDomain    │
           │     Event      │
           │   - CreditCard │
           │     CreatedEvent│
           │                │
           │ ✓ NO DEPENDE   │
           │   DE NADIE     │
           └────────────────┘
```

---

## ✅ Principios de Clean Architecture Aplicados

### **1. Regla de Dependencias**
Las dependencias apuntan **HACIA ADENTRO**, hacia las políticas de negocio:

```
API ─→ Application ─→ Domain
API ─→ Infrastructure ─→ Domain
```

### **2. Referencias de Proyectos**

#### **Domain** (Centro de la Arquitectura)
```xml
<!-- NO tiene referencias a otros proyectos -->
```

#### **Application**
```xml
<ProjectReference Include="..\CreditCard.Domain\CreditCard.Domain.csproj" />
```

#### **Infrastructure**
```xml
<ProjectReference Include="..\CreditCard.Domain\CreditCard.Domain.csproj" />
```

#### **Api**
```xml
<ProjectReference Include="..\CreditCard.Application\CreditCard.Application.csproj" />
<ProjectReference Include="..\CreditCard.Infrastructure\CreditCard.Infrastructure.csproj" />
```

---

## 📊 Flujo de Datos (Ejemplo: Crear Tarjeta)

```
1. Request HTTP (API Layer)
   ↓
2. CreditCardEndpoints.CreateCreditCard()
   ↓
3. ICreditCardService.CreateAsync() (Application Layer)
   ↓
4. CreditCardEntity.Create() (Domain Layer - Factory Method)
   ↓
5. IUnitOfWork.CreditCards.AddAsync() (Domain Interface)
   ↓
6. CreditCardRepository.AddAsync() (Infrastructure Implementation)
   ↓
7. DbContext.SaveChangesAsync()
   ↓
8. Response HTTP
```

---

## 🔄 CQRS Pattern Implementation

### **Write Side (Comandos - EF Core)**
```
API ─→ Application ─→ UnitOfWork (Domain Interface) 
                       ─→ CreditCardRepository (Infrastructure)
                       ─→ DbContext ─→ SQLite
```

### **Read Side (Consultas - Dapper)**
```
API ─→ Application ─→ ICreditCardReadRepository (Domain Interface)
                       ─→ CreditCardReadRepository (Infrastructure)
                       ─→ Dapper ─→ SQLite
```

---

## 🎯 Separación de Responsabilidades

| Capa | Responsabilidad | Tecnologías |
|------|----------------|-------------|
| **Domain** | Lógica de negocio pura, reglas de dominio | C# puro, sin dependencias |
| **Application** | Casos de uso, orquestación | DTOs, Interfaces |
| **Infrastructure** | Implementaciones técnicas | EF Core, Dapper, RabbitMQ |
| **Api** | Presentación, HTTP | Minimal APIs, Swagger |

---

## ⛔ Anti-Patrones Evitados

✗ **NO HACER:**
- Infrastructure → Application (VIOLACIÓN)
- Application → Infrastructure (VIOLACIÓN)
- Domain → Cualquier capa (VIOLACIÓN CRÍTICA)

✓ **CORRECTO:**
- Application → Domain ✓
- Infrastructure → Domain ✓
- Api → Application ✓
- Api → Infrastructure ✓

---

## 🔀 Inversión de Dependencias (DIP)

### **Ejemplo: Repositorio de Lectura**

**Domain** (Abstracción):
```csharp
public interface ICreditCardReadRepository
{
    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null);
}
```

**Infrastructure** (Implementación):
```csharp
public class CreditCardReadRepository : ICreditCardReadRepository
{
    // Implementación con Dapper
}
```

**Application** (Uso):
```csharp
public class CreditCardReportService
{
    private readonly ICreditCardReadRepository _readRepository;
    
    public CreditCardReportService(ICreditCardReadRepository readRepository)
    {
        _readRepository = readRepository; // Inversión de dependencia
    }
}
```

**Api** (Inyección):
```csharp
services.AddScoped<ICreditCardReadRepository, CreditCardReadRepository>();
```

---

## 🎁 Ventajas de Esta Arquitectura

1. ✅ **Testabilidad**: Cada capa se puede probar independientemente
2. ✅ **Mantenibilidad**: Cambios en una capa no afectan a las demás
3. ✅ **Escalabilidad**: Fácil agregar nuevas funcionalidades
4. ✅ **Reusabilidad**: La lógica de negocio es independiente de la infraestructura
5. ✅ **Flexibilidad**: Se puede cambiar la base de datos sin tocar el dominio
6. ✅ **SOLID**: Todos los principios SOLID aplicados

---

## 🚀 Próximos Pasos para Extender

1. **Agregar MediatR** ─→ Desacoplar Application con CQRS completo
2. **Agregar FluentValidation** ─→ Validaciones en Application
3. **Agregar AutoMapper** ─→ Mapeo automático de DTOs
4. **Agregar xUnit** ─→ Tests unitarios e integración
5. **Agregar Docker** ─→ Containerización

---

✨ ¡Esta es Clean Architecture en su máxima expresión! ✨
