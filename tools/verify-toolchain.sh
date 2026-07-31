#!/usr/bin/env bash
set -euo pipefail

PROJECT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
# shellcheck source=tools/toolchain.sh
source "${PROJECT_DIR}/tools/toolchain.sh"

fail() {
  echo "Toolchain verification failed: $*" >&2
  exit 1
}

DOTNET_BIN="$(command -v dotnet 2>/dev/null || true)"
[[ -n "${DOTNET_BIN}" ]] || fail "dotnet was not found on PATH"

DOTNET_VERSION="$(${DOTNET_BIN} --version 2>&1)" || fail "dotnet --version failed: ${DOTNET_VERSION}"
[[ "${DOTNET_VERSION}" == "${EXPECTED_DOTNET_VERSION}" ]] || fail "expected .NET SDK ${EXPECTED_DOTNET_VERSION}, found ${DOTNET_VERSION}"

GODOT_BIN_PATH="$(resolve_godot_bin)" || exit 1
GODOT_VERSION="$(get_godot_version "${GODOT_BIN_PATH}")" || exit 1

LOG_FILE="$(mktemp)"
trap 'rm -f "${LOG_FILE}"' EXIT
GODOT_PROJECT_DIR="$(godot_project_path "${GODOT_BIN_PATH}" "${PROJECT_DIR}")"
if ! "${GODOT_BIN_PATH}" --headless --editor --path "${GODOT_PROJECT_DIR}" --build-solutions --quit >"${LOG_FILE}" 2>&1; then
  cat "${LOG_FILE}" >&2
  fail "Godot .NET headless import/build check failed for ${GODOT_BIN_PATH}"
fi

printf 'dotnet: %s\n' "${DOTNET_VERSION}"
printf 'godot: %s\n' "${GODOT_VERSION}"
printf 'godot executable: %s\n' "${GODOT_BIN_PATH}"
printf 'headless import/build: passed\n'
