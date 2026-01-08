# ?? Guía de Pruebas - Credit Card API

Esta guía te ayudará a probar todos los endpoints de la API de Tarjetas de Crédito.

---

## ?? **Iniciar la API**

```powershell
cd src/CreditCard.Api
dotnet run
```

La API estará disponible en: `http://localhost:5282`

---

## ?? **Opciones de Testing**

### **1. Swagger UI (Recomendado para comenzar)**
Accede a: **http://localhost:5282/swagger**

Swagger proporciona una interfaz visual interactiva para probar todos los endpoints.

### **2. Visual Studio - HTTP File**
Abre el archivo: `src/CreditCard.Api/CreditCard.Api.http`

Este archivo contiene todos los endpoints listos para ejecutar directamente desde Visual Studio.

**Cómo usar:**
1. Abre `CreditCard.Api.http` en Visual Studio
2. Haz clic en "Send Request" sobre cada petición
3. Copia el `id` de las respuestas y reemplaza `{{cardId}}` en las siguientes peticiones

### **3. Postman**
Importa la colección: `CreditCard.Api.postman_collection.json`

**Características:**
- ? Scripts automáticos que guardan el `cardId`
- ? Variables de entorno pre-configuradas
- ? Tests organizados por categorías
- ? Casos de error incluidos

### **4. Script PowerShell Automatizado**
Ejecuta: `.\test-api.ps1`

Este script ejecuta automáticamente todos los tests en secuencia:
- Crea múltiples tarjetas
- Realiza operaciones de negocio
- Genera reportes
- Prueba casos de error

```powershell
.\test-api.ps1
```

### **5. cURL (Command Line)**
Ejemplos de comandos cURL para cada endpoint.

---

## ?? **Flujo de Prueba Recomendado**

### **Paso 1: Crear Tarjetas**

#### **Crear Tarjeta Visa**
```bash
curl -X POST http://localhost:5282/api/creditcards \
  -H "Content-Type: application/json" \
  -d '{
    "cardNumber": "4532015112830366",
    "cardHolderName": "Juan Pérez",
    "expirationDate": "12/2027",
    "cvv": "123",
    "creditLimit": 10000.00,
    "cardType": "Visa"
  }'
```

**Respuesta esperada (201 Created):**
```json
{
  "id": "12345678-1234-1234-1234-123456789abc",
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

**?? IMPORTANTE:** Copia el `id` de la respuesta para usarlo en las siguientes peticiones.

---

### **Paso 2: Listar Tarjetas**

```bash
curl -X GET http://localhost:5282/api/creditcards
```

**Respuesta esperada (200 OK):**
```json
[
  {
    "id": "12345678-1234-1234-1234-123456789abc",
    "cardNumber": "4532015112830366",
    "cardHolderName": "JUAN PÉREZ",
    ...
  }
]
```

---

### **Paso 3: Obtener Tarjeta por ID**

```bash
curl -X GET http://localhost:5282/api/creditcards/{cardId}
```

Reemplaza `{cardId}` con el ID real obtenido en el Paso 1.

---

### **Paso 4: Realizar Cargo**

```bash
curl -X POST http://localhost:5282/api/creditcards/{cardId}/charge \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 2500.00
  }'
```

**Respuesta esperada:**
```json
{
  "id": "12345678-1234-1234-1234-123456789abc",
  "creditLimit": 10000.00,
  "availableCredit": 7500.00,
  ...
}
```

---

### **Paso 5: Realizar Pago**

```bash
curl -X POST http://localhost:5282/api/creditcards/{cardId}/payment \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 1000.00
  }'
```

**Respuesta esperada:**
```json
{
  "availableCredit": 8500.00,
  ...
}
```

---

### **Paso 6: Ver Reportes**

#### **Reporte General**
```bash
curl -X GET http://localhost:5282/api/reports/creditcards
```

**Respuesta esperada:**
```json
[
  {
    "id": "12345678-1234-1234-1234-123456789abc",
    "cardNumber": "4532015112830366",
    "cardHolderName": "JUAN PÉREZ",
    "cardType": "Visa",
    "creditLimit": 10000.00,
    "availableCredit": 8500.00,
    "usedCredit": 1500.00,
    "usagePercentage": 15.0,
    "isActive": true
  }
]
```

#### **Tarjetas con Alto Uso (>70%)**
```bash
curl -X GET "http://localhost:5282/api/reports/creditcards/high-usage?minPercentage=70"
```

---

### **Paso 7: Actualizar Tarjeta**

```bash
curl -X PUT http://localhost:5282/api/creditcards/{cardId} \
  -H "Content-Type: application/json" \
  -d '{
    "cardHolderName": "Juan Carlos Pérez Martínez",
    "creditLimit": 12000.00
  }'
```

---

### **Paso 8: Desactivar/Activar Tarjeta**

#### **Desactivar**
```bash
curl -X POST http://localhost:5282/api/creditcards/{cardId}/deactivate
```

#### **Activar**
```bash
curl -X POST http://localhost:5282/api/creditcards/{cardId}/activate
```

---

### **Paso 9: Eliminar Tarjeta**

```bash
curl -X DELETE http://localhost:5282/api/creditcards/{cardId}
```

**Respuesta esperada (204 No Content):**
Sin contenido en el body.

---

## ?? **Casos de Error para Probar**

### **1. Número de Tarjeta Duplicado**
```bash
curl -X POST http://localhost:5282/api/creditcards \
  -H "Content-Type: application/json" \
  -d '{
    "cardNumber": "4532015112830366",
    "cardHolderName": "Test Duplicado",
    "expirationDate": "12/2027",
    "cvv": "123",
    "creditLimit": 10000.00,
    "cardType": "Visa"
  }'
```

**Respuesta esperada (400 Bad Request):**
```json
{
  "error": "Ya existe una tarjeta con este número"
}
```

---

### **2. CVV Inválido (menos de 3 dígitos)**
```bash
curl -X POST http://localhost:5282/api/creditcards \
  -H "Content-Type: application/json" \
  -d '{
    "cardNumber": "4916338506082833",
    "cardHolderName": "Test CVV",
    "expirationDate": "12/2027",
    "cvv": "12",
    "creditLimit": 10000.00,
    "cardType": "Visa"
  }'
```

**Respuesta esperada (400 Bad Request):**
```json
{
  "error": "El CVV debe tener 3 o 4 dígitos"
}
```

---

### **3. Cargo Mayor al Crédito Disponible**
```bash
curl -X POST http://localhost:5282/api/creditcards/{cardId}/charge \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 999999.00
  }'
```

**Respuesta esperada (400 Bad Request):**
```json
{
  "error": "Crédito insuficiente"
}
```

---

### **4. Operación en Tarjeta Inactiva**
Primero desactiva una tarjeta, luego intenta hacer un cargo:

```bash
# Desactivar
curl -X POST http://localhost:5282/api/creditcards/{cardId}/deactivate

# Intentar cargo
curl -X POST http://localhost:5282/api/creditcards/{cardId}/charge \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 100.00
  }'
```

**Respuesta esperada (400 Bad Request):**
```json
{
  "error": "La tarjeta está inactiva"
}
```

---

### **5. Número de Tarjeta Inválido (muy corto)**
```bash
curl -X POST http://localhost:5282/api/creditcards \
  -H "Content-Type: application/json" \
  -d '{
    "cardNumber": "123456789",
    "cardHolderName": "Test",
    "expirationDate": "12/2027",
    "cvv": "123",
    "creditLimit": 10000.00,
    "cardType": "Visa"
  }'
```

**Respuesta esperada (400 Bad Request):**
```json
{
  "error": "El número de tarjeta debe tener entre 13 y 19 dígitos"
}
```

---

## ?? **Números de Tarjeta de Prueba**

Usa estos números de tarjeta válidos para testing:

| Marca | Número | CVV | Vencimiento |
|-------|--------|-----|-------------|
| Visa | 4532015112830366 | 123 | 12/2027 |
| Visa | 4916338506082832 | 321 | 03/2029 |
| MasterCard | 5425233430109903 | 456 | 06/2026 |
| MasterCard | 5105105105105100 | 654 | 08/2028 |
| American Express | 374245455400126 | 7890 | 09/2028 |
| American Express | 378282246310005 | 1234 | 11/2027 |

---

## ?? **Escenarios de Prueba Completos**

### **Escenario 1: Ciclo de Vida Completo de una Tarjeta**

```bash
# 1. Crear tarjeta
cardId=$(curl -s -X POST http://localhost:5282/api/creditcards \
  -H "Content-Type: application/json" \
  -d '{...}' | jq -r '.id')

# 2. Realizar cargo
curl -X POST http://localhost:5282/api/creditcards/$cardId/charge \
  -H "Content-Type: application/json" \
  -d '{"amount": 2500.00}'

# 3. Realizar pago
curl -X POST http://localhost:5282/api/creditcards/$cardId/payment \
  -H "Content-Type: application/json" \
  -d '{"amount": 1000.00}'

# 4. Ver reporte
curl -X GET http://localhost:5282/api/reports/creditcards/$cardId

# 5. Desactivar
curl -X POST http://localhost:5282/api/creditcards/$cardId/deactivate

# 6. Eliminar
curl -X DELETE http://localhost:5282/api/creditcards/$cardId
```

---

### **Escenario 2: Múltiples Cargos hasta Límite**

```bash
# 1. Crear tarjeta con límite de $5000
# 2. Cargo 1: $2000 (40% usado)
# 3. Cargo 2: $1500 (70% usado)
# 4. Cargo 3: $1000 (90% usado)
# 5. Consultar reporte de alto uso
curl -X GET "http://localhost:5282/api/reports/creditcards/high-usage?minPercentage=80"
```

---

## ?? **Verificación de Base de Datos**

Puedes inspeccionar la base de datos SQLite directamente:

```bash
# Instalar sqlite3 si no lo tienes
# Windows: choco install sqlite
# Mac: brew install sqlite
# Linux: apt-get install sqlite3

# Conectar a la base de datos
sqlite3 src/CreditCard.Api/creditcards.db

# Consultas útiles
.tables
SELECT * FROM CreditCards;
SELECT CardHolderName, CreditLimit, AvailableCredit FROM CreditCards;
.exit
```

---

## ?? **Métricas de Éxito**

Al finalizar las pruebas, verifica:

- ? Todas las tarjetas creadas aparecen en GET /api/creditcards
- ? Los cargos reducen el crédito disponible correctamente
- ? Los pagos aumentan el crédito disponible
- ? No se permite exceder el límite de crédito
- ? Las validaciones de dominio funcionan (CVV, número de tarjeta, etc.)
- ? Los reportes con Dapper muestran datos correctos
- ? Las operaciones en tarjetas inactivas son rechazadas

---

## ?? **Troubleshooting**

### **Error: "Connection refused"**
Verifica que la API esté corriendo:
```bash
cd src/CreditCard.Api
dotnet run
```

### **Error: "Database locked"**
Cierra cualquier conexión SQLite abierta.

### **Error: "Invalid JSON"**
Verifica que el JSON esté bien formateado (usa un validador online).

---

¡Feliz Testing! ??
