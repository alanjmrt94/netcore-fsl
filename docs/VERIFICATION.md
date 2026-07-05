# Verificación en .NET 8

Este documento registra la verificación de compilación y pruebas de NetcoreFSL en **.NET 8**.

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (probado con **8.0.407** y **8.0.422**)
- Linux, Windows o macOS

## Comandos de verificación

Desde la raíz del repositorio:

```bash
# Restaurar dependencias
dotnet restore netcore-fsl.sln

# Compilar en Release
dotnet build netcore-fsl.sln -c Release

# Ejecutar tests automatizados
dotnet test netcore-fsl.sln -c Release
```

Salida esperada:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Passed!  - Failed: 0, Passed: 21, Skipped: 0, Total: 21
```

## Suite de pruebas (`NetcoreFSL.Tests`)

| Clase | Pruebas | Qué valida |
|-------|---------|------------|
| `FileSearchTests` | 2 | Recursión de archivos; equivalencia `.ext` / `*.ext` |
| `FolderSearchTests` | 2 | Carpetas por nombre literal y comodín |
| `SearchLifecycleTests` | 6 | `CancellationToken`, `InNewTask`, pausa/reanudación, permisos denegados (Linux) |
| `SearchPatternHelperTests` | 8 | Normalización de patrones de archivos y carpetas |
| `PathHelperTests` | 3 | Rutas extendidas Windows (`\\?\`); sin prefijo en Linux |

## Prueba manual (`NetcoreTEST`)

```bash
dotnet run --project NetcoreTEST -- /etc .conf file sync
dotnet run --project NetcoreTEST -- /etc systemd folder
```

## Nota sobre `dotnet workload update`

Si el SDK está instalado vía **Snap** (`/snap/dotnet-sdk/`), puede aparecer:

```
An issue was encountered verifying workloads.
```

y `dotnet workload update` puede fallar con *Read-only file system*. **Esto no afecta** a build, test ni run de este proyecto. NetcoreFSL no requiere workloads adicionales (MAUI, wasm, etc.).

Alternativas si desea eliminar el aviso:

1. Instalar el SDK en el directorio de usuario: <https://dot.net/v1/dotnet-install.sh>
2. Usar el paquete `dotnet-sdk-8.0` del gestor de paquetes del sistema en lugar de Snap

## Historial de verificación

| Fecha | SDK | SO | Resultado |
|-------|-----|----|-----------|
| 2026-07-05 | 8.0.407 | Linux | 15/15 tests OK |
| 2026-07-05 | 8.0.422 | Linux | 15/15 tests OK, 0 warnings Release |
| 2026-07-05 | 8.0.422 | Linux | 21/21 tests OK (pause/resume, PathHelper, fix deadlock pausa) |

## Empaquetado (opcional)

```bash
dotnet pack NetcoreFSL/NetcoreFSL.csproj -c Release
```

Genera `NetcoreFSL.1.0.5.nupkg` en `NetcoreFSL/bin/Release/`.

## Integración continua

GitHub Actions ejecuta el mismo flujo en cada push/PR a `master`/`main`. Ver [docs/CI.md](CI.md).
