# Runbook de despliegue — Azure App Service F1

> Guía autocontenida para desplegar DIDIDAI.ORG en Azure **a la primera**, sin re-investigar. Recoge todo lo
> aprendido el 2026-07-04 (incluidos los dos escollos: Norton y cuota F1). El porqué de las decisiones está
> en `decisions.md` (entrada "Despliegue en Azure App Service F1"); aquí van los pasos ejecutables.

## Datos fijos del despliegue

| Dato | Valor |
|---|---|
| Cuenta Azure | **`dididai@outlook.es`** (personal, NO la del trabajo) |
| Tenant / directorio | `dididaioutlook.onmicrosoft.com` |
| Suscripción | "Azure subscription 1" · `5c742941-32de-4787-b72b-cf092d13d81d` |
| Resource group | `rg-dididai` |
| Región | **`francecentral`** (westeurope rechaza cuentas nuevas) |
| App Service Plan | `plan-dididai` · **F1** · Linux |
| Web App | `dididai-web` · runtime `DOTNETCORE:10.0` |
| URL pública | https://dididai-web.azurewebsites.net |
| Ruta de `az` | `C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd` (puede no estar en el PATH de una shell nueva) |

> `az` no siempre está en el PATH tras instalarlo por winget. Si `az` no se reconoce, usar la ruta completa
> de arriba, o abrir una terminal nueva.

## Requisito previo CRÍTICO — Norton (interceptación TLS)

Norton re-firma el tráfico HTTPS con su raíz "Norton Web/Mail Shield", y eso rompe `az`
(`CERTIFICATE_VERIFY_FAILED`). **No** se arregla añadiendo la raíz al bundle de `az` (el cert de Norton está
mal formado para OpenSSL 3.x). Solución: **exclusiones en Norton**.

En **Norton → Configuración → Web segura → pestaña Exclusiones → Agregar**, deben estar (http y https, con `/*`):

```
login.microsoftonline.com
management.azure.com
graph.microsoft.com
*.azurewebsites.net        (necesario para el deploy: el endpoint Kudu es <app>.scm.azurewebsites.net)
```

**Cómo comprobar que Norton ya NO intercepta un host** (PowerShell): el `Issuer` debe ser Microsoft/DigiCert,
no "Norton...":

```powershell
$h = "login.microsoftonline.com"   # o dididai-web.scm.azurewebsites.net
$tcp = New-Object System.Net.Sockets.TcpClient($h, 443)
$ssl = New-Object System.Net.Security.SslStream($tcp.GetStream(), $false, ({$true}))
$ssl.AuthenticateAsClient($h)
(New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($ssl.RemoteCertificate)).Issuer
$ssl.Close(); $tcp.Close()
```

## Requisito previo — cuota F1

F1 da **~60 min de CPU/día**. Si se agota, la app pasa a estado `QuotaExceeded` ("Web App stopped", HTTP 403)
y **ni siquiera acepta deploys**; se resetea solo cada ~24h. Por eso:

- **Comprobar el estado antes de desplegar** (ver paso 0). Si está `QuotaExceeded`, esperar al reset.
- **Desplegar a la primera, sin reintentos** (cada arranque/intento gasta cuota).
- No subir a plan de pago (decisión tomada: evitar coste). F1 basta para la demo.

## Pasos

Todo desde **PowerShell**. `$az` = la ruta de la tabla.

```powershell
$az = "C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd"
```

### 0. Login y estado

```powershell
& $az login                      # navegador → dididai@outlook.es (MFA si lo pide)
& $az account set --subscription "5c742941-32de-4787-b72b-cf092d13d81d"

# ¿La app está lista para recibir deploy?
& $az webapp show --name dididai-web --resource-group rg-dididai --query "state" -o tsv
#   "Running"        -> OK, continuar
#   "QuotaExceeded"  -> cuota agotada, esperar al reset (~24h). NO intentar deploy.
```

### 1. (Solo la PRIMERA vez / si no existen) Crear recursos

> Si los recursos ya existen (caso normal a partir de ahora), **saltar al paso 3**.

```powershell
& $az group create --name rg-dididai --location francecentral
& $az appservice plan create --name plan-dididai --resource-group rg-dididai --sku F1 --is-linux
& $az webapp create --name dididai-web --resource-group rg-dididai --plan plan-dididai --runtime "DOTNETCORE:10.0"
```

### 2. (Solo la PRIMERA vez / si cambian) App settings — secretos y BD

> Los valores sensibles van aquí, NUNCA en el repo. `__` (doble guion bajo) = anidamiento de configuración .NET.

```powershell
& $az webapp config appsettings set --name dididai-web --resource-group rg-dididai --settings `
  "Seed__AdminEmail=admin@dididai.org" `
  "Seed__AdminPassword=<PONER_CONTRASEÑA>" `
  "ConnectionStrings__DefaultConnection=Data Source=/home/dididai.db"
```

`/home` es almacenamiento persistente de App Service → la SQLite sobrevive a reinicios.

### 3. Publicar (cada despliegue)

```powershell
$pub = "$env:TEMP\dididai-publish"
$zip = "$env:TEMP\dididai.zip"

dotnet publish DididaiApp/DididaiApp.csproj -c Release -o $pub
if (Test-Path $zip) { Remove-Item $zip -Force }
Compress-Archive -Path "$pub\*" -DestinationPath $zip -Force

& $az webapp deploy --name dididai-web --resource-group rg-dididai --src-path $zip --type zip
```

> El zip pesa ~28 MB; el deploy tarda 1-3 min. Es un paquete pre-compilado (no se compila en Azure), así que
> NO hace falta `SCM_DO_BUILD_DURING_DEPLOYMENT`.

### 4. Verificar

```powershell
# Home (debe dar 200)
(Invoke-WebRequest "https://dididai-web.azurewebsites.net/" -UseBasicParsing -MaximumRedirection 0).StatusCode
# /Admin sin login -> 302 a /Identity/Account/Login
# Login del admin (Seed__ ya sembrado) -> acceso a /Admin
```

La primera visita tras inactividad tarda unos segundos (la app F1 se duerme). Es normal.

## Errores conocidos y su causa

| Síntoma | Causa | Solución |
|---|---|---|
| `CERTIFICATE_VERIFY_FAILED: unable to get local issuer` | Norton intercepta ese host | Añadir el host a exclusiones de Norton (ver arriba); verificar con el snippet del Issuer |
| `Basic Constraints of CA cert not marked critical` | Se añadió la raíz Norton al bundle (no vale) | No usar esa vía; usar exclusiones de Norton |
| `RequestDisallowedByAzure - region not accepting new customers` | La región no admite cuentas nuevas | Usar `francecentral` (u otra que acepte) |
| `403 - This web app is stopped` / `state = QuotaExceeded` | Cuota diaria F1 agotada | Esperar reset (~24h); desplegar sin reintentos |
| `No subscriptions found` | Login en el tenant equivocado | Login con `dididai@outlook.es`; seleccionar la suscripción personal |
