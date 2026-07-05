#!/usr/bin/env bash
#
# Compila, empaqueta y publica NetcoreFSL en NuGet.org y GitHub Releases.
#
# Requisitos:
#   - .NET 8 SDK
#   - git
#   - gh (GitHub CLI), autenticado: gh auth login
#   - Variable NUGET_API_KEY en el entorno o en .env (ver .env.example)
#
# Uso:
#   ./scripts/release.sh              # flujo completo (pide confirmación)
#   ./scripts/release.sh --dry-run    # solo build, test y pack local
#   ./scripts/release.sh --skip-nuget # tag + release en GitHub, sin NuGet
#   ./scripts/release.sh --help

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

SOLUTION="netcore-fsl.sln"
PROJECT="NetcoreFSL/NetcoreFSL.csproj"
ARTIFACTS_DIR="$ROOT/artifacts"
CONFIGURATION="Release"

DRY_RUN=false
SKIP_NUGET=false
SKIP_GITHUB=false
SKIP_TESTS=false
PUSH_BRANCH=false
ASSUME_YES=false

NUGET_SOURCE="https://api.nuget.org/v3/index.json"

usage() {
  sed -n '2,20p' "$0" | sed 's/^# \{0,1\}//'
  echo ""
  echo "Opciones:"
  echo "  --dry-run        Compila, prueba y empaqueta sin publicar"
  echo "  --skip-nuget     Omite la publicación en NuGet.org"
  echo "  --skip-github    Omite tag y release en GitHub"
  echo "  --skip-tests     Omite dotnet test"
  echo "  --push-branch    Hace push de la rama actual antes del tag"
  echo "  --yes, -y        No pedir confirmación interactiva"
  echo "  --help, -h       Muestra esta ayuda"
}

log() {
  printf '\033[1;34m==>\033[0m %s\n' "$*"
}

warn() {
  printf '\033[1;33m!!\033[0m %s\n' "$*" >&2
}

die() {
  printf '\033[1;31mERROR:\033[0m %s\n' "$*" >&2
  exit 1
}

load_dotenv() {
  local env_file="$ROOT/.env"

  if [[ ! -f "$env_file" ]]; then
    return 0
  fi

  log "Cargando variables desde .env"

  while IFS= read -r line || [[ -n "$line" ]]; do
    line="${line#"${line%%[![:space:]]*}"}"
    [[ -z "$line" || "$line" == \#* ]] && continue

    if [[ "$line" =~ ^([A-Za-z_][A-Za-z0-9_]*)=(.*)$ ]]; then
      local key="${BASH_REMATCH[1]}"
      local value="${BASH_REMATCH[2]}"

      if [[ "$value" =~ ^\".*\"$ ]]; then
        value="${value:1:${#value}-2}"
      elif [[ "$value" =~ ^\'.*\'$ ]]; then
        value="${value:1:${#value}-2}"
      fi

      export "$key=$value"
    fi
  done <"$env_file"
}

load_dotenv

while [[ $# -gt 0 ]]; do
  case "$1" in
    --dry-run) DRY_RUN=true ;;
    --skip-nuget) SKIP_NUGET=true ;;
    --skip-github) SKIP_GITHUB=true ;;
    --skip-tests) SKIP_TESTS=true ;;
    --push-branch) PUSH_BRANCH=true ;;
    --yes|-y) ASSUME_YES=true ;;
    --help|-h) usage; exit 0 ;;
    *) die "Opción desconocida: $1 (use --help)" ;;
  esac
  shift
done

if $DRY_RUN; then
  SKIP_NUGET=true
  SKIP_GITHUB=true
fi

read_version() {
  local version
  version="$(dotnet build "$PROJECT" -getProperty:Version -v:q 2>/dev/null | tail -n1)"
  [[ -n "$version" ]] || die "No se pudo leer <Version> desde $PROJECT"
  printf '%s' "$version"
}

extract_changelog_section() {
  local version="$1"
  local changelog="$ROOT/CHANGELOG.md"

  [[ -f "$changelog" ]] || return 1

  awk -v ver="$version" '
    $0 ~ "^## \\[" ver "\\]" { found=1; next }
    found && /^## \[/ { exit }
    found { print }
  ' "$changelog" | sed '/./,$!d'
}

require_command() {
  command -v "$1" >/dev/null 2>&1 || die "Falta el comando requerido: $1"
}

confirm() {
  local prompt="$1"
  if $ASSUME_YES; then
    return 0
  fi
  read -r -p "$prompt [s/N] " reply
  [[ "$reply" =~ ^[sSyY]$ ]]
}

VERSION="$(read_version)"
TAG="v${VERSION}"
NUPKG="$ARTIFACTS_DIR/NetcoreFSL.${VERSION}.nupkg"

log "Versión detectada: $VERSION (tag $TAG)"

require_command dotnet
require_command git

if ! $SKIP_GITHUB; then
  require_command gh
  gh auth status >/dev/null 2>&1 || die "GitHub CLI no autenticado. Ejecute: gh auth login"
fi

if ! $SKIP_NUGET && ! $DRY_RUN; then
  [[ -n "${NUGET_API_KEY:-}" ]] || die "Defina NUGET_API_KEY en .env o en el entorno (ver .env.example)"
fi

if ! $DRY_RUN; then
  if [[ -n "$(git status --porcelain)" ]]; then
    die "El árbol de trabajo no está limpio. Commitee o descarte cambios antes de publicar."
  fi

  if git rev-parse "$TAG" >/dev/null 2>&1; then
    die "El tag $TAG ya existe localmente. Actualice la versión en $PROJECT o elimine el tag."
  fi

  if git ls-remote --exit-code --tags origin "$TAG" >/dev/null 2>&1; then
    die "El tag $TAG ya existe en origin."
  fi
else
  warn "Modo dry-run: se omiten comprobaciones de git limpio y tags existentes."
fi

CHANGELOG_NOTES="$(extract_changelog_section "$VERSION" || true)"
if [[ -z "$CHANGELOG_NOTES" ]]; then
  warn "No se encontró sección ## [$VERSION] en CHANGELOG.md; el release usará un mensaje genérico."
  CHANGELOG_NOTES="Release $TAG"
fi

CURRENT_BRANCH="$(git branch --show-current)"
REMOTE_URL="$(git remote get-url origin 2>/dev/null || true)"

log "Rama: ${CURRENT_BRANCH:-detached}"
[[ -n "$REMOTE_URL" ]] && log "Remoto: $REMOTE_URL"

echo ""
echo "Acciones previstas:"
echo "  1. dotnet restore / build / $([ "$SKIP_TESTS" = true ] && echo '(sin tests)' || echo test) / pack"
$SKIP_NUGET || echo "  2. Publicar $NUPKG en NuGet.org"
if ! $SKIP_GITHUB; then
  echo "  3. Crear tag $TAG y release en GitHub"
  $PUSH_BRANCH && echo "     (incluye push de la rama $CURRENT_BRANCH)"
fi
echo ""

confirm "¿Continuar?" || die "Publicación cancelada."

log "Restaurando dependencias..."
dotnet restore "$SOLUTION"

log "Compilando ($CONFIGURATION)..."
dotnet build "$SOLUTION" -c "$CONFIGURATION" --no-restore

if ! $SKIP_TESTS; then
  log "Ejecutando tests..."
  dotnet test "$SOLUTION" -c "$CONFIGURATION" --no-build --verbosity normal
else
  warn "Tests omitidos (--skip-tests)."
fi

rm -rf "$ARTIFACTS_DIR"
mkdir -p "$ARTIFACTS_DIR"

log "Empaquetando..."
dotnet pack "$PROJECT" -c "$CONFIGURATION" --no-build -o "$ARTIFACTS_DIR"

[[ -f "$NUPKG" ]] || die "No se generó el paquete esperado: $NUPKG"
log "Paquete generado: $NUPKG"

if ! $SKIP_NUGET; then
  log "Publicando en NuGet.org..."
  dotnet nuget push "$NUPKG" \
    --api-key "$NUGET_API_KEY" \
    --source "$NUGET_SOURCE" \
    --skip-duplicate
  log "NuGet.org: OK"
fi

if ! $SKIP_GITHUB; then
  if $PUSH_BRANCH; then
    log "Push de rama $CURRENT_BRANCH..."
    git push origin "HEAD:$CURRENT_BRANCH"
  fi

  log "Creando tag anotado $TAG..."
  git tag -a "$TAG" -m "Release $TAG"

  log "Push del tag..."
  git push origin "$TAG"

  RELEASE_NOTES_FILE="$(mktemp)"
  {
    echo "## NetcoreFSL $VERSION"
    echo ""
    echo "$CHANGELOG_NOTES"
    echo ""
    echo "---"
    echo "Instalación: \`dotnet add package NetcoreFSL --version $VERSION\`"
  } >"$RELEASE_NOTES_FILE"

  log "Creando GitHub Release..."
  gh release create "$TAG" \
    "$NUPKG" \
    --title "NetcoreFSL $VERSION" \
    --notes-file "$RELEASE_NOTES_FILE"

  rm -f "$RELEASE_NOTES_FILE"
  log "GitHub Release: OK"
fi

echo ""
log "Publicación completada ($TAG)."
if $DRY_RUN; then
  warn "Modo dry-run: no se publicó en NuGet ni en GitHub."
fi
