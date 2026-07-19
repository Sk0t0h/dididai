# Runbook de despliegue — Azure App Service B1 (Spain Central)

> Guía autocontenida para desplegar DIDIDAI.ORG en Azure **a la primera**, sin re-investigar. Recoge todo lo
> aprendido el 2026-07-04 (incluidos los escollos: Norton, cuota F1 y arranque de BD). El porqué de las
> decisiones está en `decisions.md` (entrada "Despliegue rehecho: B1 en Spain Central"); aquí van los pasos
> ejecutables. **El despliegue vigente es B1 en Spain (`dididai-ong`)**; lo de F1/Francia (`dididai-web`) se
> conserva como respaldo pero no es la infra activa.

## Datos fijos del despliegue

| Dato | Valor |
|---|---|
| Cuenta Azure | **`dididai@outlook.es`** (personal, NO la del trabajo) |
| Tenant / directorio | **`c074f8bf-d916-464a-9181-a755eeecbceb`** ('Directorio predeterminado'). ⚠️ **Exige MFA** → el token de `az` caduca y el deploy falla con `AADSTS50076 ... must use multi-factor authentication`; re-loguear (ver paso 0). El `dididaioutlook.onmicrosoft.com` que figuraba antes NO tiene la suscripción del TFM (login contra él da "No subscriptions found"). |
| Suscripción | **"Suscripción para el TFM"** · `5c742941-32de-4787-b72b-cf092d13d81d` (vive en el tenant `c074f8bf...`) |
| Resource group | `rg-dididai` |
| Región | **`spaincentral`** (RGPD: datos en territorio nacional) |
| App Service Plan | `plan-dididai-es` · **B1** · Linux |
| Web App | `dididai-ong` · runtime `DOTNETCORE:10.0` |
| URL pública | https://dididai-ong.azurewebsites.net |
| Respaldo (no activo) | plan `plan-dididai` F1 + webapp `dididai-web` en `francecentral` |
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

## Nota de plan — B1 (sin cuota)

El despliegue vigente usa **B1** (de pago, cubierto por el crédito): **no hay cuota diaria de CPU ni
`QuotaExceeded`**, la app no se duerme. Se puede desplegar y reintentar sin miedo a agotar nada. (El histórico
de por qué se abandonó F1 —se caía al arrancar la app— está en `decisions.md`.)

> **Pendiente del usuario:** convertir la suscripción a **Pago por uso** en el portal antes de que caduque el
> crédito Free Trial (~agosto 2026), o la suscripción se deshabilita y la web se apaga. Alerta de presupuesto
> ya creada (`presupuesto-dididai`, 30 €/mes, avisos 50%/90%).

## Pasos

Todo desde **PowerShell**. `$az` = la ruta de la tabla.

```powershell
$az = "C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd"
```

### 0. Login y estado

```powershell
& $az login                      # navegador → dididai@outlook.es + MFA (el tenant lo exige)
& $az account set --subscription "5c742941-32de-4787-b72b-cf092d13d81d"

& $az webapp show --name dididai-ong --resource-group rg-dididai --query "state" -o tsv
#   "Running" -> OK. (En B1 no aparece "QuotaExceeded".)
```

> **MFA / re-login (visto el 19-07):** el tenant `c074f8bf...` exige MFA, así que el token de `az` caduca cada
> cierto tiempo y el deploy falla con `AADSTS50076 ... must use multi-factor authentication`. Basta con
> **`& $az login`** (a secas) y, en el selector de suscripción, pulsar **Enter** (la "Suscripción para el TFM" ya
> sale marcada por defecto). ❌ NO usar `--tenant dididaioutlook.onmicrosoft.com`: ese directorio no tiene la
> suscripción y da "No subscriptions found". Tras el login, relanzar el deploy (no hace falta republicar el zip).

### 1. (Solo la PRIMERA vez / si no existen) Crear recursos

> Si los recursos ya existen (caso normal a partir de ahora), **saltar al paso 3**.

```powershell
& $az group create --name rg-dididai --location spaincentral
& $az appservice plan create --name plan-dididai-es --resource-group rg-dididai --location spaincentral --sku B1 --is-linux
& $az webapp create --name dididai-ong --resource-group rg-dididai --plan plan-dididai-es --runtime "DOTNETCORE:10.0"
```

> Si `az appservice plan create` da `No available instances to satisfy this request`, es escasez transitoria
> de esa SKU en la región; reintentar o probar otra región. En francecentral no había B1 el 04-07; Spain sí.

### 2. (Solo la PRIMERA vez / si cambian) App settings — secretos y BD

> Los valores sensibles van aquí, NUNCA en el repo. `__` (doble guion bajo) = anidamiento de configuración .NET.

```powershell
& $az webapp config appsettings set --name dididai-ong --resource-group rg-dididai --settings `
  "Seed__AdminEmail=admin@dididai.org" `
  "Seed__AdminPassword=<PONER_CONTRASEÑA>" `
  "ConnectionStrings__DefaultConnection=Data Source=/home/dididai.db"
```

`/home` es almacenamiento persistente de App Service → la SQLite sobrevive a reinicios. El esquema lo crea la
app en el arranque (`Database.MigrateAsync()` en `Program.cs`), no hay que sembrar la BD a mano.

### 3. Publicar (cada despliegue)

```powershell
$pub = "$env:TEMP\dididai-publish"
$zip = "$env:TEMP\dididai.zip"

dotnet publish DididaiApp/DididaiApp.csproj -c Release -o $pub
if (Test-Path $zip) { Remove-Item $zip -Force }
Compress-Archive -Path "$pub\*" -DestinationPath $zip -Force

& $az webapp deploy --name dididai-ong --resource-group rg-dididai --src-path $zip --type zip
```

> El zip pesa ~28 MB. Es un paquete pre-compilado (no se compila en Azure), así que NO hace falta
> `SCM_DO_BUILD_DURING_DEPLOYMENT`.
>
> **Caveat "failed" que NO es fallo:** en un arranque en frío que crea+migra la BD por primera vez, el
> contenedor puede pasarse del límite de deploy (230s) y el CLI reporta `site failed to start within 10 mins`.
> **Azure reintenta solo** y la app suele levantar en ~50s a la segunda. Antes de dar el deploy por fallido,
> **comprobar el estado real** (paso 4) y los logs (abajo): si `state=Running` y el home da 200, el deploy fue
> bien pese al mensaje de error.

### 4. Verificar

```powershell
& $az webapp show -n dididai-ong -g rg-dididai --query state -o tsv          # Running
(Invoke-WebRequest "https://dididai-ong.azurewebsites.net/" -UseBasicParsing).StatusCode   # 200
# /Admin sin login -> 302 a /Identity/Account/Login
# Login del admin (Seed__ ya sembrado) -> /Admin 200
```

### Diagnóstico — leer los logs del contenedor (cuando no arranca)

El startup log (`az webapp log startup show`) solo trae el arranque del contenedor, **no la excepción de
.NET**. Para el stdout real de la app hay que ir a Kudu, y la **basic auth de SCM suele venir deshabilitada**
(da 401). Habilitarla y descargar el log:

```powershell
& $az resource update -g rg-dididai --name scm --namespace Microsoft.Web `
  --resource-type basicPublishingCredentialsPolicies --parent "sites/dididai-ong" --set properties.allow=true
$u = & $az webapp deployment list-publishing-credentials -n dididai-ong -g rg-dididai --query publishingUserName -o tsv
$p = & $az webapp deployment list-publishing-credentials -n dididai-ong -g rg-dididai --query publishingPassword -o tsv
$b64 = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("$u`:$p"))
# listar: /api/logs/docker  ·  descargar el *_docker.log con /api/vfs/LogFiles/<nombre>
Invoke-WebRequest "https://dididai-ong.scm.azurewebsites.net/api/logs/docker" -Headers @{Authorization="Basic $b64"} -UseBasicParsing
```

## Errores conocidos y su causa

| Síntoma | Causa | Solución |
|---|---|---|
| `CERTIFICATE_VERIFY_FAILED: unable to get local issuer` | Norton intercepta ese host | Añadir el host a exclusiones de Norton (ver arriba); verificar con el snippet del Issuer |
| `Basic Constraints of CA cert not marked critical` | Se añadió la raíz Norton al bundle (no vale) | No usar esa vía; usar exclusiones de Norton |
| `RequestDisallowedByAzure - region not accepting new customers` | La región no admite cuentas nuevas | Usar una región que acepte (spaincentral funcionó) |
| `No available instances to satisfy this request` | Escasez transitoria de esa SKU en la región | Reintentar o cambiar de región (B1 no estaba en francecentral, sí en spaincentral) |
| `site failed to start within 10 mins` en el deploy, pero luego `state=Running` | Primer arranque en frío (crear+migrar BD) supera el timeout de deploy; Azure reintenta solo | Verificar estado real; si Running + home 200, ignorar el mensaje |
| Crash de arranque sin migrar (worker no levanta, BD vacía) | Seed antes de aplicar migraciones con `/home` vacío | Ya resuelto: `Database.MigrateAsync()` antes del seed en `Program.cs` |
| `401 Unauthorized` al leer logs de Kudu | Basic auth de SCM deshabilitada | Habilitarla con `az resource update ... basicPublishingCredentialsPolicies ... --set properties.allow=true` |
| `403 - This web app is stopped` / `state = QuotaExceeded` | (Solo aplicaba a F1) cuota diaria agotada | No aplica en B1; era el motivo de abandonar F1 |
| `No subscriptions found` | Login en el tenant equivocado | Login con `dididai@outlook.es`; seleccionar la suscripción personal |
