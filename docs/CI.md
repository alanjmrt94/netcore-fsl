# Integración continua (GitHub Actions)

El repositorio usa [GitHub Actions](https://docs.github.com/en/actions) para build, tests y publicación en NuGet.

## Workflows

| Archivo | Disparador | Qué hace |
|---------|------------|----------|
| `.github/workflows/ci.yml` | Push / PR a `master` o `main` | `restore` → `build` (Release) → `test` → `pack` (smoke) |
| `.github/workflows/release.yml` | Push de tag `v*.*.*` | `pack` → publicación en [NuGet.org](https://www.nuget.org/) |

## Badge de estado

```markdown
![CI](https://github.com/alanjmrt94/netcore-fsl/actions/workflows/ci.yml/badge.svg?branch=master)
```

## Publicar una versión en NuGet

1. Asegurar que `<Version>` en `NetcoreFSL/NetcoreFSL.csproj` coincide con el tag (sin prefijo `v`).
2. Actualizar `CHANGELOG.md` con la entrada de la versión.
3. En GitHub → **Settings** → **Secrets and variables** → **Actions**, crear `NUGET_API_KEY` (para el workflow `release.yml` en CI).

   Para publicar **desde tu máquina** con `scripts/release.sh`, use un archivo `.env` en la raíz del repo (no versionado):

```bash
cp .env.example .env
# Editar .env y pegar la API key de https://www.nuget.org/account/apikeys
./scripts/release.sh --push-branch
```

4. Crear y pushear el tag:

```bash
git tag -a v1.0.3 -m "Release v1.0.3"
git push origin v1.0.3
```

El workflow `Release` empaqueta y sube `NetcoreFSL.<version>.nupkg`. `--skip-duplicate` evita fallar si el paquete ya existe.

## Script local (`scripts/release.sh`)

Alternativa al workflow de GitHub para publicar desde tu máquina en un solo paso:

```bash
export NUGET_API_KEY="su-api-key"   # opcional si usa .env
./scripts/release.sh --push-branch
```

O con `.env` (recomendado localmente):

```bash
cp .env.example .env
# NUGET_API_KEY=... en .env
./scripts/release.sh --push-branch
```

| Opción | Descripción |
|--------|-------------|
| `--dry-run` | Solo `restore`, `build`, `test` y `pack` (sin publicar) |
| `--skip-nuget` | Tag + release en GitHub, sin NuGet.org |
| `--skip-github` | Solo publicar en NuGet.org |
| `--push-branch` | Push de la rama actual antes de crear el tag |
| `--yes` | Sin confirmación interactiva |

**Requisitos:** .NET 8 SDK, `git`, `gh` autenticado (`gh auth login`), variable `NUGET_API_KEY`.

El script lee la versión desde `NetcoreFSL/NetcoreFSL.csproj`, exige árbol git limpio (salvo en `--dry-run`), extrae las notas de `CHANGELOG.md` para el release de GitHub y adjunta el `.nupkg`.

## Verificación local (equivalente al CI)

```bash
dotnet restore netcore-fsl.sln
dotnet build netcore-fsl.sln -c Release --no-restore
dotnet test netcore-fsl.sln -c Release --no-build
dotnet pack NetcoreFSL/NetcoreFSL.csproj -c Release --no-build
```
