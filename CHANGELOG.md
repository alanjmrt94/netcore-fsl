# Changelog

Todos los cambios notables de este proyecto se documentan en este archivo.

El formato se basa en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/),
y el proyecto adhiere a [Semantic Versioning](https://semver.org/lang/es/).

## [Unreleased]

### Added
- Plan de desarrollo (`DEVELOPMENT_PLAN.md`).
- Clase `FSLVersion` y propiedad `FSL.Version` para consultar la versión en tiempo de ejecución.
- Metadatos de versión y paquete en `NetcoreFSL.csproj`.
- `NetcoreTEST` parametrizable por CLI y variables de entorno.

### Changed
- Refactor de `Fsl` / `FileSearch` a fachada `FSL` con `FileSearch()` y `FolderSearch()`.
- Renombrado `DriveFolderEventArgs.cs` → `DriveEventArgs.cs`.
- Métodos `On*` en `SearcherBase` para disparar eventos.

## [0.1.0] - 2026-07-05

### Added
- Estructura inicial de la biblioteca: `SearcherBase`, `FilePatternSearch`, eventos y `ExecuteHandlers`.
- Proyecto de prueba `NetcoreTEST`.

[Unreleased]: https://github.com/alanjmrt94/netcore-fsl/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/alanjmrt94/netcore-fsl/releases/tag/v0.1.0
