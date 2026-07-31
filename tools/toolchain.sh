#!/usr/bin/env bash

TOOLCHAIN_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
GODOT_VERSION_FILE="${TOOLCHAIN_ROOT}/tools/godot-version.txt"
EXPECTED_DOTNET_VERSION="10.0.302"

if [[ ! -f "${GODOT_VERSION_FILE}" ]]; then
  echo "Godot version file not found: ${GODOT_VERSION_FILE}" >&2
  return 1
fi

EXPECTED_GODOT_VERSION="$(tr -d '\r\n' < "${GODOT_VERSION_FILE}")"

normalize_godot_version() {
  local raw_version="${1-}"
  local normalized
  local escaped_expected
  local version_pattern

  raw_version="${raw_version//$'\r'/}"
  normalized="$(printf '%s' "${raw_version}" | sed -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//')"

  escaped_expected="$(printf '%s' "${EXPECTED_GODOT_VERSION}" | sed 's/[.]/\\./g')"
  version_pattern="^${escaped_expected}\.(official|custom_build)(\.[[:alnum:]]+)?$"
  if [[ "${normalized}" =~ ${version_pattern} ]]; then
    printf '%s.%s\n' "${EXPECTED_GODOT_VERSION}" "${BASH_REMATCH[1]}"
    return 0
  fi
  return 1
}

resolve_godot_bin() {
  local candidate
  local name

  if [[ -n "${GODOT_BIN:-}" ]]; then
    candidate="${GODOT_BIN}"
    if [[ -f "${candidate}" ]]; then
      printf '%s\n' "${candidate}"
      return 0
    fi
    if candidate="$(command -v "${candidate}" 2>/dev/null)"; then
      printf '%s\n' "${candidate}"
      return 0
    fi
    echo "GODOT_BIN does not point to an executable: ${GODOT_BIN}" >&2
    return 1
  fi

  for name in godot4-mono godot-mono godot4 godot Godot.exe godot.exe; do
    if candidate="$(command -v "${name}" 2>/dev/null)"; then
      printf '%s\n' "${candidate}"
      return 0
    fi
  done

  echo "No Godot .NET editor found. Set GODOT_BIN or add one of the supported commands to PATH: godot4-mono, godot-mono, godot4, godot, Godot.exe, godot.exe" >&2
  return 1
}

get_godot_version() {
  local godot_bin="$1"
  local raw_version
  local normalized

  if ! raw_version="$("${godot_bin}" --version 2>&1)"; then
    echo "Godot executable failed when queried with --version: ${godot_bin}" >&2
    return 1
  fi
  if ! normalized="$(normalize_godot_version "${raw_version}")"; then
    echo "Unsupported Godot version from ${godot_bin}: ${raw_version}" >&2
    return 1
  fi
  printf '%s\n' "${normalized}"
}

verify_godot_dotnet_editor() {
  local godot_bin="$1"
  local help_output

  if ! help_output="$("${godot_bin}" --help 2>&1)"; then
    echo "Godot executable failed when queried with --help: ${godot_bin}" >&2
    return 1
  fi
  if ! printf '%s\n' "${help_output}" | grep -Fq -- '--build-solutions'; then
    echo "Godot executable does not expose the .NET editor option --build-solutions: ${godot_bin}" >&2
    return 1
  fi
}

godot_project_path() {
  local godot_bin="$1"
  local project_dir="$2"

  if [[ "${godot_bin}" == *.exe ]] && command -v wslpath >/dev/null 2>&1; then
    wslpath -w "${project_dir}"
  elif [[ "${godot_bin}" == *.exe ]] && command -v cygpath >/dev/null 2>&1; then
    cygpath -w "${project_dir}"
  else
    printf '%s\n' "${project_dir}"
  fi
}
