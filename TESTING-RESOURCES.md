# ?? Recursos de Testing - Credit Card API

## ?? Archivos de Testing Disponibles

### **1. CreditCard.Api.http** ? (Visual Studio)
?? `src/CreditCard.Api/CreditCard.Api.http`

**Características:**
- ? 16 endpoints pre-configurados
- ? Datos de prueba incluidos
- ? Casos de error documentados
- ? Funciona directamente en Visual Studio 2022+

**Cómo usar:**
1. Abre el archivo en Visual Studio
2. Clic derecho en cualquier petición
3. Selecciona "Send Request"
4. Copia el `id` de la respuesta para las siguientes peticiones

---

### **2. CreditCard.Api.postman_collection.json** ? (Postman)
?? `CreditCard.Api.postman_collection.json`

**Características:**
- ? Colección completa con 30+ peticiones
- ? Scripts automáticos que guardan variables
- ? Organizado por categorías
- ? Tests de error incluidos

**Cómo importar:**
1. Abre Postman
2. File ? Import
3. Selecciona `CreditCard.Api.postman_collection.json`
4. La colección aparecerá en tu sidebar

---

### **3. test-api.ps1** ? (PowerShell Script)
?? `test-api.ps1`

**Características:**
- ? Testing automatizado completo
- ? Colores en la consola para mejor visualización
- ? Prueba todos los endpoints en secuencia
- ? Incluye timing entre peticiones

**Cómo ejecutar:**
```powershell
# En PowerShell
.\test-api.ps1

# Si tienes problemas de permisos:
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\test-api.ps1
```

---

### **4. TESTING-GUIDE.md** ?? (Documentación)
?? `TESTING-GUIDE.md`

**Características:**
- ? Guía paso a paso completa
- ? Ejemplos con cURL
- ? Casos de error explicados
- ? Números de tarjeta de prueba
- ? Escenarios completos de testing

---

## ?? Inicio Rápido

### **Opción 1: Swagger UI (Más Fácil)**
```bash
# 1. Iniciar API
cd src/CreditCard.Api
dotnet run

# 2. Abrir navegador
http://localhost:5282/swagger
```

### **Opción 2: Visual Studio HTTP File**
```bash
# 1. Iniciar API
cd src/CreditCard.Api
dotnet run

# 2. En Visual Studio
# Abrir: src/CreditCard.Api/CreditCard.Api.http
# Click en "Send Request" sobre cada endpoint
```

### **Opción 3: Script Automático**
```powershell
# 1. Iniciar API
cd src/CreditCard.Api
dotnet run

# 2. En otra terminal
.\test-api.ps1
```

---

## ?? Endpoints Disponibles

### **CRUD Operations (8 endpoints)**
- `POST /api/creditcards` - Crear tarjeta
- `GET /api/creditcards` - Listar todas
- `GET /api/creditcards/{id}` - Obtener por ID
- `PUT /api/creditcards/{id}` - Actualizar
- `DELETE /api/creditcards/{id}` - Eliminar
- `POST /api/creditcards/{id}/charge` - Realizar cargo
- `POST /api/creditcards/{id}/payment` - Realizar pago
- `POST /api/creditcards/{id}/activate` - Activar
- `POST /api/creditcards/{id}/deactivate` - Desactivar

### **Reports - Dapper (4 endpoints)**
- `GET /api/reports/creditcards` - Reporte completo
- `GET /api/reports/creditcards/{id}` - Reporte individual
- `GET /api/reports/creditcards/active` - Tarjetas activas
- `GET /api/reports/creditcards/high-usage?minPercentage=X` - Alto uso

---

## ?? Datos de Prueba Rápidos

### **Tarjeta Visa**
```json
{
  "cardNumber": "4532015112830366",
  "cardHolderName": "Juan Pérez",
  "expirationDate": "12/2027",
  "cvv": "123",
  "creditLimit": 10000.00,
  "cardType": "Visa"
}
```

### **Tarjeta MasterCard**
```json
{
  "cardNumber": "5425233430109903",
  "cardHolderName": "María García",
  "expirationDate": "06/2026",
  "cvv": "456",
  "creditLimit": 15000.00,
  "cardType": "MasterCard"
}
```

### **Cargo**
```json
{
  "amount": 500.00
}
```

### **Pago**
```json
{
  "amount": 300.00
}
```

---

## ? Checklist de Testing

- [ ] ? Crear tarjeta Visa
- [ ] ? Crear tarjeta MasterCard
- [ ] ? Crear tarjeta American Express
- [ ] ? Listar todas las tarjetas
- [ ] ? Obtener tarjeta por ID
- [ ] ? Actualizar nombre de titular
- [ ] ? Actualizar límite de crédito
- [ ] ? Realizar cargo
- [ ] ? Realizar pago
- [ ] ? Desactivar tarjeta
- [ ] ? Activar tarjeta
- [ ] ? Eliminar tarjeta
- [ ] ? Reporte completo (Dapper)
- [ ] ? Reporte individual (Dapper)
- [ ] ? Tarjetas activas (Dapper)
- [ ] ? Alto uso de crédito (Dapper)
- [ ] ?? Error: Número duplicado
- [ ] ?? Error: CVV inválido
- [ ] ?? Error: Número de tarjeta inválido
- [ ] ?? Error: Cargo excesivo
- [ ] ?? Error: Operación en tarjeta inactiva

---

## ?? URLs Importantes

| Recurso | URL |
|---------|-----|
| **API Base** | http://localhost:5282 |
| **Swagger UI** | http://localhost:5282/swagger |
| **OpenAPI JSON** | http://localhost:5282/openapi/v1.json |

---

## ?? Variables de Entorno

```bash
# HTTP File (Visual Studio)
@CreditCard.Api_HostAddress = http://localhost:5282
@cardId = {tu-guid-aqui}

# Postman
baseUrl = http://localhost:5282
cardId = {se guarda automáticamente}

# PowerShell
$baseUrl = "http://localhost:5282"
$cardId = "{obtenido de respuestas}"
```

---

## ?? Respuestas Esperadas

### **201 Created (POST /api/creditcards)**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "cardNumber": "4532015112830366",
  "cardHolderName": "JUAN PÉREZ",
  "expirationDate": "12/2027",
  "cardType": "Visa",
  "creditLimit": 10000.00,
  "availableCredit": 10000.00,
  "isActive": true,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": null
}
```

### **200 OK (GET /api/creditcards)**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "cardNumber": "4532015112830366",
    "cardHolderName": "JUAN PÉREZ",
    "expirationDate": "12/2027",
    "cardType": "Visa",
    "creditLimit": 10000.00,
    "availableCredit": 9500.00,
    "isActive": true,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T11:00:00Z"
  }
]
```

### **200 OK (GET /api/reports/creditcards - Dapper)**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "cardNumber": "4532015112830366",
    "cardHolderName": "JUAN PÉREZ",
    "cardType": "Visa",
    "creditLimit": 10000.00,
    "availableCredit": 9500.00,
    "usedCredit": 500.00,
    "usagePercentage": 5.0,
    "isActive": true
  }
]
```

### **400 Bad Request (Error)**
```json
{
  "error": "Ya existe una tarjeta con este número"
}
```

### **404 Not Found**
```json
{
  "error": "Tarjeta no encontrada"
}
```

### **204 No Content (DELETE)**
Sin contenido en el body.

---

## ??? Herramientas Recomendadas

| Herramienta | Propósito | Link |
|-------------|-----------|------|
| **Visual Studio 2022** | Ejecutar .http files | https://visualstudio.com |
| **Postman** | API Testing | https://postman.com |
| **SQLite Browser** | Inspeccionar DB | https://sqlitebrowser.org |
| **curl** | Command line testing | Pre-instalado en Windows 10+ |
| **jq** | JSON parsing en terminal | https://stedolan.github.io/jq/ |

---

## ?? Solución de Problemas

### **Error: "Connection refused"**
```bash
# Verifica que la API esté corriendo
cd src/CreditCard.Api
dotnet run
```

### **Error: "Cannot execute script"**
```powershell
# PowerShell: Cambiar política de ejecución
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
```

### **Error: "Database is locked"**
```bash
# Cierra SQLite Browser si está abierto
# Reinicia la API
```

### **Error: "Port 5282 already in use"**
```bash
# Cambia el puerto en appsettings.json o launchSettings.json
# O mata el proceso que usa el puerto:
netstat -ano | findstr :5282
taskkill /PID {PID} /F
```

---

## ?? Documentación Adicional

- ?? [DOCUMENTATION.md](DOCUMENTATION.md) - Documentación completa de la API
- ??? [ARCHITECTURE.md](ARCHITECTURE.md) - Arquitectura Clean + DDD
- ?? [TESTING-GUIDE.md](TESTING-GUIDE.md) - Guía detallada de testing
- ?? [README.md](README.md) - Información general del proyecto

---

## ?? Ejemplos de Uso por Caso

### **Caso 1: Testing Rápido en 2 Minutos**
1. `dotnet run` en `src/CreditCard.Api`
2. Abrir http://localhost:5282/swagger
3. Probar "POST /api/creditcards"
4. Copiar el `id` de la respuesta
5. Probar "GET /api/creditcards/{id}"

### **Caso 2: Testing Exhaustivo con Script**
1. `dotnet run` en `src/CreditCard.Api`
2. En otra terminal: `.\test-api.ps1`
3. Observar todos los tests ejecutarse automáticamente

### **Caso 3: Testing Manual con Postman**
1. `dotnet run` en `src/CreditCard.Api`
2. Importar `CreditCard.Api.postman_collection.json`
3. Ejecutar "Create Visa Card"
4. El `cardId` se guarda automáticamente
5. Ejecutar el resto de peticiones en orden

---

¡Todo listo para probar! ??
