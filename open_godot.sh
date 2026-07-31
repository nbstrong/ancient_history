#!/usr/bin/env bash
set -euo pipefail

PROJECT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
ENGINE="${PROJECT_DIR}/../../../Godot_v4.3-stable_mono_win64/Godot_v4.3-stable_mono_win64.exe"

if [[ ! -f "$ENGINE" ]]; then
  echo "Godot editor not found: $ENGINE" >&2
  exit 1
fi

# The bundled editor is a Windows executable. Convert the project path when
# running from WSL or Git Bash so Godot receives a native Windows path.
if command -v wslpath >/dev/null 2>&1; then
  GODOT_PROJECT_DIR="$(wslpath -w "$PROJECT_DIR")"
elif command -v cygpath >/dev/null 2>&1; then
  GODOT_PROJECT_DIR="$(cygpath -w "$PROJECT_DIR")"
else
  GODOT_PROJECT_DIR="$PROJECT_DIR"
fi

exec "$ENGINE" --editor --path "$GODOT_PROJECT_DIR" "$@"
