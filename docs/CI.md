# Integración continua (GitHub Actions)

El repositorio usa [GitHub Actions](https://docs.github.com/en/actions) para build, tests y publicación en NuGet.

## Workflows

Los workflows usan acciones oficiales en **Node 24** (`@v5`) y `FORCE_JAVASCRIPT_ACTIONS_TO_NODE24: true` a nivel de workflow.

| Archivo | Disparador | Qué hace |
|---------|------------|----------|
| `.github/workflows/ci.yml` | Push / PR a `master` o `main` | `restore` → `build` (Release) → `test` → `pack` (smoke) |
| `.github/workflows/release.yml` | Push de tag `v*.*.*` | build → pack → firma opcional → publicación en [NuGet.org](https://www.nuget.org/) |

## Badge de estado

```markdown
![CI](https://github.com/alanjmrt94/netcore-fsl/actions/workflows/ci.yml/badge.svg?branch=master)
```

## Publicar una versión en NuGet

1. Asegurar que `<Version>` en `NetcoreFSL/NetcoreFSL.csproj` coincide con el tag (sin prefijo `v`).
2. Actualizar `CHANGELOG.md` con la entrada de la versión.

### GitHub Actions → NuGet.org (Trusted Publishing)

El workflow `release.yml` usa [Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing) (OIDC). No se usan API keys permanentes.

#### 1. Environment en GitHub (recomendado)

Los secrets de publicación van en un **Environment**, no a nivel de repositorio. Así podés limitar quién publica y restringir despliegues a tags.

**GitHub** → repo → **Settings** → **Environments** → **New environment**

| Campo | Valor |
|-------|--------|
| Name | `nuget-publish` |

Dentro del environment → **Environment secrets**:

| Secret | ¿Obligatorio? | Descripción |
|--------|---------------|-------------|
| `NUGET_USER` | **Sí** | Username de nuget.org (Trusted Publishing) |
| `EMZAPPS_SNK` | No | Strong-name `.snk` en **base64** (mismo que [NETFastSearchLibrary](https://github.com/alanjmrt94/netcore-fsl)) |
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

**nuget.org** → cuenta → **Trusted Publishing** → crear política:

| Campo | Valor |
|-------|--------|
| Owner (GitHub) | `alanjmrt94` |
| Repository | `netcore-fsl` |
| Workflow File | `release.yml` |

#### 3. Tag y push

```bash
git tag -a v1.0.5 -m "Release v1.0.5"
git push origin v1.0.5
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

## Verificación local (equivalente al CI)

```bash
dotnet restore netcore-fsl.sln
dotnet build netcore-fsl.sln -c Release --no-restore
dotnet test netcore-fsl.sln -c Release --no-build
dotnet pack NetcoreFSL/NetcoreFSL.csproj -c Release --no-build
```
