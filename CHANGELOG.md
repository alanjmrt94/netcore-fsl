# Changelog

Todos los cambios notables de este proyecto se documentan en este archivo.

El formato se basa en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/),
y el proyecto adhiere a [Semantic Versioning](https://semver.org/lang/es/).

## [Unreleased]

## [1.0.6] - 2026-07-05

### Fixed
- Warning CS1700 en builds con strong-name: `InternalsVisibleTo` omitido en `OfficialBuild` (release no compila tests).
- Documentación de Trusted Publishing: alinear Repository Owner, Environment y troubleshooting del error 401.

## [1.0.5] - 2026-07-05

### Added
- Firma strong-name opcional con `EMZApps.snk` (`OfficialBuild=true`, mismo token que NETFastSearchLibrary).
- Author Signing opcional del `.nupkg` (Authenticode) en `release.yml`.
- NuGet Trusted Publishing (`NuGet/login@v1`) y environment `nuget-publish` en GitHub.

### Changed
- GitHub Actions: acciones `@v5` (Node 24) y `FORCE_JAVASCRIPT_ACTIONS_TO_NODE24`.
- `release.sh`: solo build local + tag + GitHub Release; NuGet vía CI (Trusted Publishing).
- `InternalsVisibleTo` condicional para builds con strong-name (solo en CI).

### Removed
- Publicación local con `NUGET_API_KEY` y `.env.example` (redundante con Trusted Publishing).

## [1.0.3] - 2026-07-05

### Added
- GitHub Actions: workflow `CI` (restore, build, test, pack) en push/PR a `master`/`main`.
- Workflow `Release` para publicar en NuGet.org al pushear tags `v*.*.*`.
- `docs/CI.md` con instrucciones de badge, secrets y publicación.
- `scripts/release.sh` para publicar localmente (build, test, NuGet y GitHub Release).

## [1.0.2] - 2026-07-05

### Added
- Tests automatizados de `PauseSearch` / `ResumeSearch`.
- `PathHelper` con prefijo `\\?\` en Windows para rutas largas.

### Fixed
- Deadlock al pausar: el semáforo de concurrencia ya no se mantiene durante el recorrido paralelo de subdirectorios.

## [1.0.1] - 2026-07-05

### Added
- `global.json` y `.editorconfig` para builds reproducibles en .NET 8.
- Configuración VS Code: `launch.json`, `tasks.json` y `extensions.json` para `NetcoreTEST`.

### Changed
- `.gitignore` depurado para .NET 8, tests, cobertura y `.cursor/` (local).
- `LICENSE` y metadatos NuGet: copyright `2021-2026`.
- Descripción del paquete actualizada a .NET 8.

## [1.0.0] - 2026-07-05

### Added
- Documentación completa: README reescrito, `docs/VERIFICATION.md` con verificación .NET 8.
- Comentarios XML (`///`) en toda la API pública (`FSL`, `FSLVersion`, `ExecuteHandlers`, eventos).

### Changed
- Versión estable `1.0.0` — Fases 0–5 completadas.
- `GenerateDocumentationFile` habilitado en `NetcoreFSL.csproj`.

## [0.5.0] - 2026-07-05

### Added
- Proyecto `NetcoreFSL.Tests` con xUnit: recursión, patrones, cancelación, `InNewTask`, permisos (Linux).
- `InternalsVisibleTo` para pruebas de `SearchPatternHelper`.

### Changed
- Migración de `net6.0` a `net8.0` en todos los proyectos.
- `Nullable` habilitado en `NetcoreFSL`.
- Metadatos NuGet ampliados (`RepositoryType`, `PackageTags`, `PackageReadmeFile`).

## [0.4.0] - 2026-07-05

### Added
- `FolderPatternSearch` con búsqueda recursiva de carpetas por patrón de nombre.
- `SearchPatternHelper.NormalizeFile` y `NormalizeFolder` para patrones de archivos y carpetas.
- Evento `FoldersFound` funcional en `FolderSearch()`.
- `NetcoreTEST`: modo `folder`, handler `FoldersFound` y ejemplos en comentarios.

### Changed
- `SearcherBase`: hook `ProcessDirectory` reemplaza `GetFiles`; `GetDrives` y `GetFolders` con implementación base compartida.
- `README.md`: ejemplo de `FolderSearch` y tabla de funcionalidades actualizada.

## [0.3.0] - 2026-07-05

### Added
- Todos los eventos de `SearcherBase` expuestos en `FSL` (`DrivesFound`, `FoldersFound`, `SearchCanceled`, `SearchPaused`, `SearchResumed`).
- Constructor `FSL(..., CancellationToken)` para cancelación externa.
- `CancelSearch()`, `PauseSearch()` y `ResumeSearch()` en `FSL` y `SearcherBase`.
- Soporte de `ExecuteHandlers.InNewTask` (búsqueda en `Task.Run`).
- `NetcoreTEST`: argumento `sync|async`, variable `FSL_TIMEOUT_MS` y eventos de ciclo de vida.

### Changed
- Nueva búsqueda cancela automáticamente la anterior en la misma instancia `FSL`.
- `README.md`: tabla de API, limitaciones y ejemplos de cancelación/async.

## [0.2.0] - 2026-07-05

### Added
- Clase `FSLVersion` y propiedad `FSL.Version` para consultar la versión en tiempo de ejecución.
- Metadatos de versión y paquete en `NetcoreFSL.csproj` (`PackageId`, `Authors`, licencia MIT).
- `NetcoreTEST` parametrizable por CLI y variables de entorno.
- `SearchPatternHelper` para normalizar patrones (`.pdf` → `*.pdf`).
- Búsqueda recursiva de archivos con paralelismo acotado (`SemaphoreSlim`, `ConcurrentDictionary`).
- Eventos `FilesFound` y `SearchCompleted` expuestos en `FSL`.
- Detección de ciclos en el árbol de directorios (rutas visitadas).
- `CHANGELOG.md` con formato Keep a Changelog.

### Changed
- Refactor de `Fsl` / `FileSearch` a fachada `FSL` con `FileSearch()` y `FolderSearch()`.
- Renombrado `DriveFolderEventArgs.cs` → `DriveEventArgs.cs`.
- Métodos `On*` en `SearcherBase` para disparar eventos de forma thread-safe.
- `RunFSL` reescrito: orquesta raíces, recursión completa y `SearchCompleted`.
- `README.md` ampliado con uso básico, estado por funcionalidad y política de versionado.

### Fixed
- Bug en `GetFolders` que ignoraba directorios con una sola subcarpeta (`Length > 1` → `> 0`).

## [0.1.0] - 2026-07-05

### Added
- Estructura inicial de la biblioteca: `SearcherBase`, `FilePatternSearch`, eventos y `ExecuteHandlers`.
- Proyecto de prueba `NetcoreTEST`.

[Unreleased]: https://github.com/alanjmrt94/netcore-fsl/compare/v1.0.6...HEAD
[1.0.6]: https://github.com/alanjmrt94/netcore-fsl/compare/v1.0.5...v1.0.6
[1.0.5]: https://github.com/alanjmrt94/netcore-fsl/compare/v1.0.3...v1.0.5
[1.0.3]: https://github.com/alanjmrt94/netcore-fsl/compare/v1.0.2...v1.0.3
[1.0.2]: https://github.com/alanjmrt94/netcore-fsl/compare/v1.0.1...v1.0.2
[1.0.1]: https://github.com/alanjmrt94/netcore-fsl/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/alanjmrt94/netcore-fsl/compare/v0.5.0...v1.0.0
[0.5.0]: https://github.com/alanjmrt94/netcore-fsl/compare/v0.4.0...v0.5.0
[0.4.0]: https://github.com/alanjmrt94/netcore-fsl/compare/v0.3.0...v0.4.0
[0.3.0]: https://github.com/alanjmrt94/netcore-fsl/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/alanjmrt94/netcore-fsl/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/alanjmrt94/netcore-fsl/releases/tag/v0.1.0
