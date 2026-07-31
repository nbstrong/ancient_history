#!/usr/bin/env bash
set -euo pipefail

PROJECT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=tools/toolchain.sh
source "${PROJECT_DIR}/tools/toolchain.sh"

GODOT_BIN_PATH="$(resolve_godot_bin)"
GODOT_VERSION="$(get_godot_version "${GODOT_BIN_PATH}")"
printf 'Using Godot %s: %s\n' "${GODOT_VERSION}" "${GODOT_BIN_PATH}" >&2

GODOT_PROJECT_DIR="$(godot_project_path "${GODOT_BIN_PATH}" "${PROJECT_DIR}")"
exec "${GODOT_BIN_PATH}" --editor --path "${GODOT_PROJECT_DIR}" "$@"
