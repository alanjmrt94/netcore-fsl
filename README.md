# NetcoreFSL

**Versión:** `0.2.0` · **Estado:** WIP — búsqueda de archivos funcional (Fases 0–1)  
**aka:** NetCore FastSearchLibrary

Biblioteca multiplataforma y multi-hilo para .NET 6, escrita en C#, que permite buscar archivos y directorios mediante patrones.

## Qué incluye esta versión

| Funcionalidad | Estado |
|---------------|--------|
| Búsqueda recursiva de archivos por patrón | Disponible |
| Eventos `FilesFound` y `SearchCompleted` | Disponible |
| Normalización de patrones (`.conf` → `*.conf`) | Disponible |
| Búsqueda de carpetas (`FolderSearch`) | Pendiente (Fase 3) |
| Cancelación, pausa y `InNewTask` | Pendiente (Fase 2) |

## Versión en tiempo de ejecución

```csharp
using NetcoreFSL;

Console.WriteLine(FSL.Version);        // "0.2.0"
Console.WriteLine(FSLVersion.Current); // "0.2.0"
```

La versión se define en `NetcoreFSL/NetcoreFSL.csproj` y se expone en código mediante `FSL.Version` y `FSLVersion`.

## Uso básico

```csharp
using NetcoreFSL;
using NetcoreFSL.Searcher.Enums;

var fsl = new FSL(ExecuteHandlers.InCurrentTask, "/etc", ".conf");

fsl.FilesFound += (_, e) =>
{
  foreach (var file in e.Files)
    Console.WriteLine(file.FullName);
};

fsl.SearchCompleted += (_, e) =>
  Console.WriteLine($"Completado: {e.IsCompleted}");

fsl.FileSearch();
```

### Proyecto de prueba

```bash
dotnet run --project NetcoreTEST -- /etc .conf file
```

Argumentos: `<carpeta> <patron> [file|folder]`. Variables de entorno: `FSL_FOLDER`, `FSL_PATTERN`, `FSL_MODE`.

## Política de versionado

Se sigue [Semantic Versioning](https://semver.org/):

| Segmento | Cuándo incrementar |
|----------|-------------------|
| **MAJOR** | Cambios incompatibles en la API pública |
| **MINOR** | Nueva funcionalidad compatible (p. ej. cierre de una fase del plan) |
| **PATCH** | Correcciones de bugs compatibles |

Durante el desarrollo pre-1.0 (`0.x.y`), las versiones **MINOR** marcan hitos del [plan de desarrollo](DEVELOPMENT_PLAN.md):

| Versión | Hito |
|---------|------|
| `0.1.0` | Fase 0 — saneamiento |
| `0.2.0` | Fase 1 — búsqueda de archivos funcional |
| `0.3.0` | Fase 3 — búsqueda de carpetas |
| `1.0.0` | Release estable |

### Mensajes de commit con versión

```
chore: complete phase 0 cleanup (v0.1.0)
feat: implement recursive file search (v0.2.0)
release: v1.0.0
```

Al publicar una versión, actualizar:

1. `NetcoreFSL/NetcoreFSL.csproj` → `<Version>`
2. `CHANGELOG.md` → nueva entrada
3. `README.md` → línea de versión al inicio
4. Tag git: `git tag v0.2.0`

## Requisitos

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) o superior

## Licencia

[MIT](LICENSE) — Copyright (c) 2021 Alan Javier Martinez
