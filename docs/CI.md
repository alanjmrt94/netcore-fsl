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
3. En GitHub → **Settings** → **Secrets and variables** → **Actions**, crear `NUGET_API_KEY` con una API key de [nuget.org](https://www.nuget.org/account/apikeys).
4. Crear y pushear el tag:

```bash
git tag -a v1.0.3 -m "Release v1.0.3"
git push origin v1.0.3
```

El workflow `Release` empaqueta y sube `NetcoreFSL.<version>.nupkg`. `--skip-duplicate` evita fallar si el paquete ya existe.

## Verificación local (equivalente al CI)

```bash
dotnet restore netcore-fsl.sln
dotnet build netcore-fsl.sln -c Release --no-restore
dotnet test netcore-fsl.sln -c Release --no-build
dotnet pack NetcoreFSL/NetcoreFSL.csproj -c Release --no-build
```
