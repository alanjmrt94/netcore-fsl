# NetcoreFSL

**Versión:** `1.0.0` · **Estado:** estable (Fases 0–5)  
**aka:** NetCore FastSearchLibrary

Biblioteca multiplataforma y multi-hilo para **.NET 8**, escrita en C#, que permite buscar archivos y directorios en el sistema de archivos mediante patrones, con API basada en eventos.

## Características

- Búsqueda recursiva de **archivos** y **carpetas** por patrón
- API basada en **eventos** (`FilesFound`, `FoldersFound`, ciclo de vida)
- Ejecución síncrona o en segundo plano (`ExecuteHandlers`)
- **Cancelación**, pausa y reanudación
- Paralelismo acotado y seguro entre hilos
- **15 tests automatizados** (xUnit), verificados en .NET 8

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) o superior

## Instalación

### Referencia al proyecto (desarrollo)

```bash
git clone https://github.com/alanjmrt94/netcore-fsl.git
cd netcore-fsl
dotnet build netcore-fsl.sln
```

En su `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="ruta/a/netcore-fsl/NetcoreFSL/NetcoreFSL.csproj" />
</ItemGroup>
```

### Paquete NuGet (cuando esté publicado)

```bash
dotnet add package NetcoreFSL
```

## Inicio rápido

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

## API pública

### Clase `FSL`

| Miembro | Descripción |
|---------|-------------|
| `FSL.Version` | Versión semántica (`1.0.0`) |
| `FSL(handler, folder, pattern)` | Constructor |
| `FSL(handler, folder, pattern, cancellationToken)` | Constructor con cancelación |
| `FileSearch()` | Búsqueda recursiva de archivos |
| `FolderSearch()` | Búsqueda recursiva de carpetas |
| `CancelSearch()` | Cancela la búsqueda activa |
| `PauseSearch()` | Pausa la búsqueda activa |
| `ResumeSearch()` | Reanuda la búsqueda pausada |

### Eventos

| Evento | Cuándo se dispara |
|--------|-------------------|
| `DrivesFound` | Al enumerar unidades (`folder` = `""`, `"*"` o `"/"`) |
| `FilesFound` | Archivos que coinciden con el patrón |
| `FoldersFound` | Carpetas que coinciden con el patrón |
| `SearchCanceled` | Al cancelar |
| `SearchCompleted` | Al finalizar (`IsCompleted` = éxito) |
| `SearchPaused` / `SearchResumed` | Al pausar / reanudar |

### `ExecuteHandlers`

| Valor | Comportamiento |
|-------|----------------|
| `InCurrentTask` | Bloquea el hilo llamador hasta finalizar |
| `InNewTask` | Ejecuta en `Task.Run` y retorna de inmediato |

### Patrones

| Tipo | Entrada | Normalizado |
|------|---------|-------------|
| Archivo | `.pdf` | `*.pdf` |
| Archivo | `*.log` | `*.log` |
| Carpeta | `systemd` | `systemd` |
| Carpeta | `cache*` | `cache*` |

### Comportamiento entre búsquedas

- Los eventos **persisten** en la instancia `FSL`.
- Una nueva `FileSearch()` / `FolderSearch()` **cancela** la búsqueda anterior.
- Use **instancias separadas** para búsquedas concurrentes independientes.

## Ejemplos

### Búsqueda de carpetas

```csharp
var fsl = new FSL(ExecuteHandlers.InCurrentTask, "/etc", "systemd");
fsl.FoldersFound += (_, e) =>
{
  foreach (var dir in e.Folders)
    Console.WriteLine(dir.FullName);
};
fsl.FolderSearch();
```

### Cancelación y segundo plano

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var fsl = new FSL(ExecuteHandlers.InNewTask, "/var/log", ".log", cts.Token);
fsl.SearchCompleted += (_, e) => Console.WriteLine(e.IsCompleted);
fsl.FileSearch(); // retorna de inmediato
```

## Verificación y pruebas

### Tests automatizados (.NET 8)

```bash
dotnet test netcore-fsl.sln -c Release
```

**Resultado verificado:** 15/15 pruebas OK, 0 errores, 0 warnings (Release).

Detalle en [docs/VERIFICATION.md](docs/VERIFICATION.md).

### Proyecto de prueba manual

```bash
dotnet run --project NetcoreTEST -- /etc .conf file sync
dotnet run --project NetcoreTEST -- /etc systemd folder async
```

Argumentos: `<carpeta> <patron> [file|folder] [sync|async]`

Variables de entorno: `FSL_FOLDER`, `FSL_PATTERN`, `FSL_MODE`, `FSL_HANDLER`, `FSL_TIMEOUT_MS`

## Limitaciones conocidas

| Limitación | Detalle |
|------------|---------|
| Symlinks | Pueden generar visitas duplicadas; se detectan ciclos por ruta visitada |
| Rutas largas (Windows) | Prefijo `\\?\` no implementado aún |
| `InNewTask` | Excepciones no se propagan al hilo llamador; use `SearchCompleted` |
| Workloads Snap | SDK vía Snap puede mostrar aviso de workloads; no afecta este proyecto |

## Estructura del repositorio

```
netcore-fsl/
├── NetcoreFSL/          # Biblioteca principal
├── NetcoreFSL.Tests/    # Tests xUnit (15 pruebas)
├── NetcoreTEST/         # Consola de prueba manual
├── docs/VERIFICATION.md # Verificación .NET 8
├── CHANGELOG.md
└── DEVELOPMENT_PLAN.md
```

## Versión en tiempo de ejecución

```csharp
Console.WriteLine(FSL.Version);        // "1.0.0"
Console.WriteLine(FSLVersion.Current); // "1.0.0"
```

Fuente de verdad: `NetcoreFSL/NetcoreFSL.csproj` → `<Version>`.

## Versionado

[Semantic Versioning](https://semver.org/). Historial en [CHANGELOG.md](CHANGELOG.md).

| Versión | Hito |
|---------|------|
| `1.0.0` | Release estable — Fases 0–5 |
| `0.5.0` | Tests y migración .NET 8 |
| `0.4.0` | Búsqueda de carpetas |
| `0.3.0` | API de ciclo de vida |

Plan completo: [DEVELOPMENT_PLAN.md](DEVELOPMENT_PLAN.md).

## Licencia

[MIT](LICENSE) — Copyright (c) 2021 Alan Javier Martinez
