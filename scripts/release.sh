#!/usr/bin/env bash
#
# Release de NetcoreFSL:
#   1. Compilar, probar y empaquetar (Release) en local
#   2. Crear tag git, push y GitHub Release (adjunta el .nupkg)
#   3. NuGet.org vía Trusted Publishing (release.yml al recibir el tag)
#
# Requisitos:
#   - .NET 8 SDK
#   - git (árbol limpio salvo en --dry-run)
#   - gh autenticado (gh auth login) — salvo --skip-github
#   - Environment nuget-publish en GitHub (NUGET_USER) + Trusted Publishing en nuget.org
#
# Uso:
#   ./scripts/release.sh               # build + tag + GitHub Release → CI publica en NuGet
#   ./scripts/release.sh --dry-run     # solo build, test y pack local
#   ./scripts/release.sh --skip-github # solo build/pack (sin tag ni Release)
#   ./scripts/release.sh --push-branch # push de rama antes del tag
#   ./scripts/release.sh --help

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

SOLUTION="netcore-fsl.sln"
PROJECT="NetcoreFSL/NetcoreFSL.csproj"
ARTIFACTS_DIR="$ROOT/artifacts"
CONFIGURATION="Release"

DRY_RUN=false
SKIP_GITHUB=false
SKIP_TESTS=false
PUSH_BRANCH=false
ASSUME_YES=false

usage() {
  sed -n '2,20p' "$0" | sed 's/^# \{0,1\}//'
  echo ""
  echo "Opciones:"
  echo "  --dry-run      Solo compila, prueba y empaqueta (sin tag ni Release)"
  echo "  --skip-github  Omite tag y GitHub Release"
  echo "  --skip-tests   Omite dotnet test"
  echo "  --push-branch  Push de la rama actual antes de crear el tag"
  echo "  --yes, -y      Sin confirmación interactiva"
  echo "  --help, -h     Muestra esta ayuda"
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

while [[ $# -gt 0 ]]; do
  case "$1" in
    --dry-run) DRY_RUN=true; SKIP_GITHUB=true ;;
    --skip-github) SKIP_GITHUB=true ;;
    --skip-tests) SKIP_TESTS=true ;;
    --push-branch) PUSH_BRANCH=true ;;
    --yes|-y) ASSUME_YES=true ;;
    --help|-h) usage; exit 0 ;;
    *) die "Opción desconocida: $1 (use --help)" ;;
  esac
  shift
done

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
  warn "Modo dry-run: se omiten comprobaciones de git y publicación."
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
echo "Pipeline de release:"
echo "  1. Local: restore → build → $([ "$SKIP_TESTS" = true ] && echo '(sin tests)' || echo test) → pack"
if ! $SKIP_GITHUB; then
  echo "  2. GitHub: tag $TAG → push → Release (adjunta .nupkg)"
  $PUSH_BRANCH && echo "     (incluye push previo de rama $CURRENT_BRANCH)"
  echo "  3. CI: release.yml → NuGet/login (Trusted Publishing) → nuget.org"
else
  warn "Sin tag: NuGet.org no se publicará (Trusted Publishing requiere push del tag)."
fi
echo ""

confirm "¿Continuar?" || die "Publicación cancelada."

log "[1/3] Restaurando dependencias..."
dotnet restore "$SOLUTION"

log "[1/3] Compilando ($CONFIGURATION)..."
dotnet build "$SOLUTION" -c "$CONFIGURATION" --no-restore

if ! $SKIP_TESTS; then
  log "[1/3] Ejecutando tests..."
  dotnet test "$SOLUTION" -c "$CONFIGURATION" --no-build --verbosity normal
else
  warn "Tests omitidos (--skip-tests)."
fi

rm -rf "$ARTIFACTS_DIR"
mkdir -p "$ARTIFACTS_DIR"

log "[1/3] Empaquetando..."
dotnet pack "$PROJECT" -c "$CONFIGURATION" --no-build -o "$ARTIFACTS_DIR"

[[ -f "$NUPKG" ]] || die "No se generó el paquete esperado: $NUPKG"
log "Paquete generado: $NUPKG"

if ! $SKIP_GITHUB; then
  if $PUSH_BRANCH; then
    log "[2/3] Push de rama $CURRENT_BRANCH..."
    git push origin "HEAD:$CURRENT_BRANCH"
  fi

  log "[2/3] Creando tag anotado $TAG..."
  git tag -a "$TAG" -m "Release $TAG"

  log "[2/3] Push del tag..."
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

  log "[2/3] Creando GitHub Release..."
  gh release create "$TAG" \
    "$NUPKG" \
    --title "NetcoreFSL $VERSION" \
    --notes-file "$RELEASE_NOTES_FILE"

  rm -f "$RELEASE_NOTES_FILE"
  log "GitHub Release: OK"
  log "[3/3] NuGet.org: release.yml (Trusted Publishing). Ver: gh run list --workflow=release.yml"
fi

echo ""
if $DRY_RUN; then
  log "Dry-run completado ($TAG)."
else
  log "Release completado ($TAG)."
  if ! $SKIP_GITHUB; then
    log "El paquete firmado se publicará en NuGet.org vía GitHub Actions."
  fi
fi
