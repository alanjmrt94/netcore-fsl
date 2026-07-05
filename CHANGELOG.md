# Changelog

Todos los cambios notables de este proyecto se documentan en este archivo.

El formato se basa en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/),
y el proyecto adhiere a [Semantic Versioning](https://semver.org/lang/es/).

## [Unreleased]

### Added
- (pendiente Fase 2) Cancelación, pausa/reanudación y `ExecuteHandlers.InNewTask`.
- (pendiente Fase 3) Búsqueda de carpetas con `FolderSearch()`.

## [0.2.0] - 2026-07-05

### Added
- Plan de desarrollo (`DEVELOPMENT_PLAN.md`).
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

[Unreleased]: https://github.com/alanjmrt94/netcore-fsl/compare/v0.2.0...HEAD
[0.2.0]: https://github.com/alanjmrt94/netcore-fsl/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/alanjmrt94/netcore-fsl/releases/tag/v0.1.0
