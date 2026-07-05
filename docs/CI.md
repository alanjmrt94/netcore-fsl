# Integración continua (GitHub Actions)

El repositorio usa [GitHub Actions](https://docs.github.com/en/actions) para build, tests y publicación en NuGet.

## Workflows

| Archivo | Disparador | Qué hace |
|---------|------------|----------|
| `.github/workflows/ci.yml` | Push / PR a `master` o `main` | `restore` → `build` (Release) → `test` → `pack` (smoke) |
| `.github/workflows/release.yml` | Push de tag `v*.*.*` | build → pack → firma opcional → publicación en [NuGet.org](https://www.nuget.org/) |

## Node.js 24 en los runners

GitHub deprecó Node.js 20 en los runners. Ambos workflows declaran:

```yaml
env:
  FORCE_JAVASCRIPT_ACTIONS_TO_NODE24: true
```

Eso fuerza la ejecución en Node 24 de acciones que aún declaran Node 20. Preferimos acciones que ya usen Node 24 de forma nativa para evitar warnings.

| Acción | Versión | Runtime |
|--------|---------|---------|
| `actions/checkout` | `@v5` | Node 24 |
| `actions/setup-dotnet` | `@v5` | Node 24 |
| `actions/cache` | `@v5` | Node 24 |
| `actions/upload-artifact` | `@v5` | Node 24 |
| `nuget/setup-nuget` | `@v4` | Node 24 |
| `NuGet/login` | `@v1` | Node 24 |

> **Nota:** `nuget/setup-nuget@v2` apuntaba a Node 20 y generaba warnings aun con `FORCE_JAVASCRIPT_ACTIONS_TO_NODE24`. `@v4` es la versión actual con runtime Node 24.

## Badge de estado

```markdown
![CI](https://github.com/alanjmrt94/netcore-fsl/actions/workflows/ci.yml/badge.svg?branch=master)
```

## Publicar una versión en NuGet

1. Asegurar que `<Version>` en `NetcoreFSL/NetcoreFSL.csproj` coincide con el tag (sin prefijo `v`).
2. Actualizar `CHANGELOG.md` con la entrada de la versión.

### GitHub Actions → NuGet.org (Trusted Publishing)

El workflow `release.yml` usa [Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing) (OIDC). No se usan API keys permanentes.

#### 1. Environment en GitHub

Los secrets de publicación van en un **Environment**, no a nivel de repositorio. Así podés limitar quién publica y restringir despliegues a tags.

**GitHub** → repo → **Settings** → **Environments** → **New environment**

| Campo | Valor |
|-------|--------|
| Name | `nuget-publish` |

Dentro del environment → **Environment secrets**:

| Secret | ¿Obligatorio? | Descripción |
|--------|---------------|-------------|
| `NUGET_USER` | **Sí** | Username **nuget.org de quien creó la política** de Trusted Publishing (no el owner del paquete si difieren). Ver [solución de problemas](#solución-de-problemas-trusted-publishing) |
| `EMZAPPS_SNK` | No | Strong-name `.snk` en **base64** (mismo que NETFastSearchLibrary) |
| `NUGET_SIGN_CERT_PFX` | No | Certificado Authenticode `.pfx` en **base64** |
| `NUGET_SIGN_CERT_PASSWORD` | No | Contraseña del `.pfx` (requerida si hay certificado) |

Sin `NUGET_SIGN_CERT_*`: el `.nupkg` se publica **sin Author Signing**; nuget.org aplica **Repository Signing** (igual que el proyecto legacy por defecto).

Exportar certificado a base64:

```bash
base64 -w0 /ruta/codigo-firma.pfx   # Linux
```

**Opcional — reglas de protección** (misma pantalla del environment):

- **Deployment branches:** *Selected branches and tags* → añadir patrón `v*` (solo tags de release).
- **Required reviewers:** activar si querés aprobación manual antes de publicar.

El job en `release.yml` declara `environment: nuget-publish`; solo ese job accede a esos secrets.

#### 2. Política en nuget.org

**nuget.org** → cuenta → **Trusted Publishing** → crear o editar política.

Los valores deben coincidir **exactamente** con el repo y el workflow (nuget.org valida el token OIDC de GitHub campo por campo):

| Campo en nuget.org | Valor |
|--------------------|--------|
| Package owner | `alanjmrt94` |
| Repository Owner (GitHub) | `alanjmrt94` |
| Repository | `netcore-fsl` |
| Workflow File | `release.yml` (solo el nombre, sin `.github/workflows/`) |
| Environment | `nuget-publish` (o vacío si no querés restringir por environment) |

Errores frecuentes:

- **Repository Owner** distinto al owner real del repo (p. ej. `EMZ-Apps` cuando el repo es `alanjmrt94/netcore-fsl`).
- **Environment** distinto al declarado en `release.yml` (p. ej. `prod` vs `nuget-publish`).

#### 3. Tag y push

```bash
git tag -a v1.0.6 -m "Release v1.0.6"
git push origin v1.0.6
```

O usar el script local (ver abajo), que crea el tag y dispara el workflow.

El workflow empaqueta, firma opcionalmente (`nuget sign`), obtiene una credencial efímera vía `NuGet/login@v1` y sube `NetcoreFSL.<version>.nupkg`. `--skip-duplicate` evita fallar si el paquete ya existe.

### Firma del paquete (solo en CI)

| Tipo | Qué firma | Secret en `nuget-publish` | Por defecto |
|------|-----------|----------------------------|-------------|
| **Strong-name** | Ensamblado `NetcoreFSL.dll` | `EMZAPPS_SNK` (base64) | Desactivado |
| **Author Signing** | `.nupkg` (Authenticode) | `NUGET_SIGN_CERT_PFX` + `NUGET_SIGN_CERT_PASSWORD` | Desactivado |
| **Repository Signing** | `.nupkg` en nuget.org | Automático al publicar | **Sí** |

`ci.yml` y builds locales compilan sin strong-name. Solo `release.yml` aplica firma cuando los secrets están configurados.

> **Nota:** No activar «Require signing by a registered certificate» en nuget.org hasta registrar el certificado Authenticode.

### Script local (`scripts/release.sh`)

Valida el build local, crea tag y GitHub Release; **NuGet.org lo publica CI** vía Trusted Publishing.

| Paso | Acción |
|------|--------|
| 1 | **Local:** `restore` → `build` → `test` → `pack` |
| 2 | **GitHub:** tag `v*` → push → `gh release create` (adjunta `.nupkg`) |
| 3 | **CI:** `release.yml` → `NuGet/login` (OIDC) → `dotnet nuget push` |

```bash
./scripts/release.sh --push-branch
gh run list --workflow=release.yml
```

| Opción | Descripción |
|--------|-------------|
| `--dry-run` | Solo compila, prueba y empaqueta |
| `--skip-github` | Solo build/pack local |
| `--push-branch` | Push de la rama antes del tag |
| `--yes` | Sin confirmación interactiva |

**Requisitos:** .NET 8 SDK, `git`, `gh auth login`, environment `nuget-publish` con `NUGET_USER`.

El script exige árbol git limpio (salvo `--dry-run`), lee la versión de `NetcoreFSL.csproj` y las notas de `CHANGELOG.md`.

## Solución de problemas (Trusted Publishing)

### Error 401: «No matching trust policy owned by user …»

El paso `NuGet/login@v1` falla si no hay una política que coincida con el token OIDC. Revisá en este orden:

1. **`NUGET_USER`:** debe ser el username de nuget.org de quien **creó** la política (no necesariamente el package owner).
2. **Repository Owner:** debe ser `alanjmrt94` (owner del repo en GitHub).
3. **Repository:** `netcore-fsl`.
4. **Workflow file:** `release.yml`.
5. **Environment:** `nuget-publish` (o dejá el campo vacío en nuget.org).

En GitHub → **Settings** → **Environments** → `nuget-publish` → verificá `NUGET_USER`.

Volvé a ejecutar el workflow:

```bash
gh run list --workflow=release.yml
gh run rerun <run-id> --failed
```

### Warning CS1700 en `release.yml` (InternalsVisibleTo)

Los builds con `OfficialBuild=true` (release en CI) no compilan tests; `InternalsVisibleTo` solo aplica en builds locales/CI sin strong-name. Si apareciera de nuevo, revisá que no haya un `PublicKey` truncado en el `.csproj`.

### Warning «Node.js 20 is deprecated»

Actualizá las acciones JavaScript a versiones con runtime Node 24 (ver tabla arriba). Con `nuget/setup-nuget@v4` y el resto en `@v5`, no deberían quedar warnings de deprecación.

## Verificación local (equivalente al CI)

```bash
dotnet restore netcore-fsl.sln
dotnet build netcore-fsl.sln -c Release --no-restore
dotnet test netcore-fsl.sln -c Release --no-build
dotnet pack NetcoreFSL/NetcoreFSL.csproj -c Release --no-build
```
