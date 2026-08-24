# WhatsApp Cloud API — Setup Guide (Meta)

## Prerrequisitos
- Meta Business Account (WABA) verificada
- Número de teléfono verificado en WhatsApp Manager
- Token de acceso de larga duración (permanent token o system user token)
- App Secret desde App Dashboard

## 1. Crear Templates

En **WhatsApp Manager** → **Message Templates** → **Create Template**:

### Template 1: `appointment_reminder` (con botones)

- **Name:** `appointment_reminder`
- **Category:** Utility
- **Language:** Spanish (Mexico) — `es_MX`
- **Body:**
```
Hola {{1}}, te recordamos que tienes una cita programada el {{2}} a las {{3}} en {{4}}.
Por favor llega 15 minutos antes.
```
- **Buttons (en este orden exacto):**
  1. Quick Reply: "Confirmar"
  2. Quick Reply: "Cancelar"
  3. URL: "Reagendar" → `https://portal.clinicflow.com.mx/reschedule/{{1}}`

Variables:
| Position | Description | Example |
|----------|-------------|---------|
| `{{1}}` | Nombre del paciente | Juan García |
| `{{2}}` | Fecha (dd/MM/yyyy) | 20/01/2026 |
| `{{3}}` | Hora (HH:mm) | 10:00 |
| `{{4}}` | Nombre de la clínica | Clínica Centro |

### Template 2: `appointment_confirmation` (sin botones)

- **Name:** `appointment_confirmation`
- **Category:** Utility
- **Language:** Spanish (Mexico) — `es_MX`
- **Body:**
```
Hola {{1}}, tu cita ha sido confirmada para el {{2}} a las {{3}} en {{4}}.
Te esperamos.
```
- **Buttons:** Ninguno

### Template 3: `appointment_cancelled` (sin botones)

- **Name:** `appointment_cancelled`
- **Category:** Utility
- **Language:** Spanish (Mexico) — `es_MX`
- **Body:**
```
Hola {{1}}, tu cita del {{2}} a las {{3}} en {{4}} ha sido cancelada.
Si deseas reagendar, puedes hacerlo desde tu portal o contactarnos.
```
- **Buttons:** Ninguno

## 2. Obtener Credenciales

1. **PhoneNumberId:** WhatsApp Manager → Settings → Phone numbers → copiar ID
2. **AccessToken:** Meta for Developers → System Users → Generate Token (permisos: `whatsapp_business_messaging`)
3. **AppSecret:** App Dashboard → Settings → Basic → App Secret

## 3. Configurar en el Proyecto

### Opción A: User Secrets (desarrollo local)
```bash
cd Backend/Services/MedPalApi/MedPal.API

dotnet user-secrets set "WhatsApp:AccessToken" "TU_TOKEN_AQUI"
dotnet user-secrets set "WhatsApp:AppSecret" "TU_APP_SECRET"
dotnet user-secrets set "WhatsApp:PhoneNumberId" "TU_PHONE_NUMBER_ID"
dotnet user-secrets set "WhatsApp:WebhookVerifyToken" "tu-token-verificacion"
dotnet user-secrets set "WhatsApp:Enabled" "true"
```

### Opción B: Variables de entorno (producción/Docker)
```bash
export WhatsApp__AccessToken="TU_TOKEN_AQUI"
export WhatsApp__AppSecret="TU_APP_SECRET"
export WhatsApp__PhoneNumberId="TU_PHONE_NUMBER_ID"
export WhatsApp__WebhookVerifyToken="tu-token-verificacion"
export WhatsApp__Enabled="true"
```

### Valores no sensibles (appsettings.json)
```json
"WhatsApp": {
  "Enabled": true,
  "GraphUrl": "https://graph.facebook.com",
  "ApiVersion": "v21.0",
  "TemplateName": "appointment_reminder",
  "ConfirmationTemplateName": "appointment_confirmation",
  "CancelledTemplateName": "appointment_cancelled",
  "TemplateLanguage": "es_MX",
  "RescheduleBaseUrl": "https://portal.clinicflow.com.mx/reschedule",
  "ReminderHour": 18,
  "ReminderWindowHoursAhead": 24,
  "CheckIntervalMinutes": 30,
  "HttpTimeoutSeconds": 30
}
```

### Botones (solo appointment_reminder)

El template `appointment_reminder` incluye 3 botones:
- **Quick Reply "Confirmar"** → paciente confirma la cita (status → Confirmed)
- **Quick Reply "Cancelar"** → paciente cancela la cita (status → Cancelled)
- **URL "Reagendar"** → abre el portal del paciente en la página de reagendación

Los templates `appointment_confirmation` y `appointment_cancelled` solo tienen body (sin botones).

## 4. Números de Prueba

En **WhatsApp Manager** → **Phone numbers** → **Insights** → **Add test numbers** (hasta 5).

Los números deben:
- Estar en formato: `+521XXXXXXXXXX`
- Tener WhatsApp instalado
- Enviar un mensaje de prueba desde el Business Manager primero

## 5. Webhook (Opcional — para estado de entrega)

Para recibir confirmaciones de entrega/lectura:

1. Configurar URL pública (ngrok o dominio propio): `POST https://tu-dominio/api/webhooks/whatsapp`
2. En Meta App Dashboard → WhatsApp → Configuration → Webhook:
   - Callback URL: `https://tu-dominio/api/webhooks/whatsapp`
   - Verify Token: el mismo configurado en `WhatsApp:WebhookVerifyToken`
3. Suscribir eventos: `messages` y `statuses`

Para desarrollo local con ngrok:
```bash
ngrok http 5126
# Copiar la URL https://XXXX.ngrok-free.app
# Usar como callback URL
```

## 6. Consentimiento de Pacientes

El sistema requiere **opt-in explícito** del paciente:
- Checkbox "Acepto recibir recordatorios por WhatsApp" en formulario de paciente
- Campo `IsWhatsAppConsented` en la tabla `Patients`
- El job de recordatorios **solo envía** si: `IsWhatsAppConsented == true && IsMarketingBlocked == false && Phone != null`

## 7. Envío Manual

Desde el detalle de paciente → botón "Enviar Recordatorio":
- Busca la próxima cita del paciente
- Valida consentimiento y teléfono
- Envía template vía WhatsApp Cloud API
- Registra en `NotificationMessages`

Endpoint: `POST /api/appointments/{id}/reminder`

## 8. Job Automático

`AppointmentReminderJob` corre cada 30 minutos:
- Consulta citas para mañana (ventana configurable)
- Filtra: Status=Scheduled, `ReminderSentAt=null`, consentimiento OK
- Envía recordatorio → marca `ReminderSentAt` para evitar duplicados
