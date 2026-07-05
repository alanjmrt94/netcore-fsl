# NetcoreFSL

**Versión:** `0.3.0` · **Estado:** WIP — Fases 0–2 completadas  
**aka:** NetCore FastSearchLibrary

Biblioteca multiplataforma y multi-hilo para .NET 6, escrita en C#, que permite buscar archivos y directorios mediante patrones.

## Qué incluye esta versión

| Funcionalidad | Estado |
|---------------|--------|
| Búsqueda recursiva de archivos por patrón | Disponible |
| Eventos de resultados y ciclo de vida | Disponible |
| `ExecuteHandlers.InNewTask` (búsqueda en segundo plano) | Disponible |
| `CancellationToken`, `CancelSearch()` | Disponible |
| `PauseSearch()` / `ResumeSearch()` | Disponible |
| Normalización de patrones (`.conf` → `*.conf`) | Disponible |
| Búsqueda de carpetas (`FolderSearch`) | Pendiente (Fase 3) |

## API pública (`FSL`)

| Miembro | Descripción |
|---------|-------------|
| `FSL.Version` | Versión semántica de la biblioteca |
| `FSL(handler, folder, pattern)` | Constructor síncrono |
| `FSL(handler, folder, pattern, cancellationToken)` | Constructor con cancelación externa |
| `FileSearch()` | Inicia búsqueda de archivos |
| `FolderSearch()` | Inicia búsqueda de carpetas (Fase 3) |
| `CancelSearch()` | Cancela la búsqueda en curso |
| `PauseSearch()` | Pausa la búsqueda en curso |
| `ResumeSearch()` | Reanuda la búsqueda pausada |

### Eventos

| Evento | Cuándo se dispara |
|--------|-------------------|
| `DrivesFound` | Al enumerar unidades (búsqueda en todas las drives) |
| `FilesFound` | Al encontrar archivos que coinciden con el patrón |
| `FoldersFound` | Al encontrar carpetas (Fase 3) |
| `SearchCanceled` | Al cancelar la búsqueda |
| `SearchCompleted` | Al finalizar (éxito o cancelación) |
| `SearchPaused` | Al pausar |
| `SearchResumed` | Al reanudar |

### `ExecuteHandlers`

| Valor | Comportamiento |
|-------|----------------|
| `InCurrentTask` | `FileSearch()` / `FolderSearch()` bloquean el hilo llamador |
| `InNewTask` | La búsqueda se ejecuta en `Task.Run` y retorna de inmediato |

### Suscripciones y búsquedas múltiples

- Los eventos se suscriben en la instancia `FSL` y **persisten** entre llamadas.
- Cada nueva llamada a `FileSearch()` o `FolderSearch()` **cancela** la búsqueda anterior si sigue activa.
- Se recomienda usar **instancias separadas** de `FSL` para búsquedas concurrentes independientes.

## Versión en tiempo de ejecución

```csharp
using NetcoreFSL;

Console.WriteLine(FSL.Version);        // "0.3.0"
Console.WriteLine(FSLVersion.Current); // "0.3.0"
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

### Cancelación y ejecución en segundo plano

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

var fsl = new FSL(ExecuteHandlers.InNewTask, "/var/log", ".log", cts.Token);

fsl.SearchCompleted += (_, e) => Console.WriteLine(e.IsCompleted);
fsl.SearchCanceled += (_, e) => Console.WriteLine(e.IsCanceled);

fsl.FileSearch(); // retorna de inmediato

// fsl.CancelSearch(); // o cancelar manualmente
```

### Proyecto de prueba

```bash
dotnet run --project NetcoreTEST -- /etc .conf file sync
dotnet run --project NetcoreTEST -- /etc .conf file async
FSL_TIMEOUT_MS=5000 dotnet run --project NetcoreTEST -- / usr
```

Argumentos: `<carpeta> <patron> [file|folder] [sync|async]`.

Variables de entorno: `FSL_FOLDER`, `FSL_PATTERN`, `FSL_MODE`, `FSL_HANDLER`, `FSL_TIMEOUT_MS`.

## Limitaciones conocidas

- `FolderSearch()` aún no está implementado (Fase 3).
- Los symlinks pueden provocar visitas duplicadas; se detectan ciclos por ruta canónica visitada.
- En Windows, rutas muy largas pueden requerir el prefijo `\\?\` (no implementado aún).
- `InNewTask` no propaga excepciones al hilo llamador; suscríbase a `SearchCompleted`.

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
| `0.2.0` | Fases 0–1 — búsqueda de archivos |
| `0.3.0` | Fase 2 — API de ciclo de vida |
| `0.4.0` | Fase 3 — búsqueda de carpetas |
| `1.0.0` | Release estable |

### Mensajes de commit con versión

```
chore: complete phase 0 cleanup (v0.1.0)
feat: implement recursive file search (v0.2.0)
feat: add search lifecycle API (v0.3.0)
release: v1.0.0
```

Al publicar una versión, actualizar:

1. `NetcoreFSL/NetcoreFSL.csproj` → `<Version>`
2. `CHANGELOG.md` → nueva entrada
3. `README.md` → línea de versión al inicio
4. Tag git: `git tag v0.x.0`

## Requisitos

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) o superior

## Licencia

[MIT](LICENSE) — Copyright (c) 2021 Alan Javier Martinez
