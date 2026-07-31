#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)"
# shellcheck source=tools/toolchain.sh
source "${ROOT}/tools/toolchain.sh"

TEMP_DIR="$(mktemp -d)"
trap 'rm -rf "${TEMP_DIR}"' EXIT

failures=0
pass() { printf 'PASS: %s\n' "$1"; }
fail() { printf 'FAIL: %s\n' "$1" >&2; failures=$((failures + 1)); }

assert_normalizes() {
  local input="$1"
  local expected="$2"
  local actual
  if actual="$(normalize_godot_version "${input}")" && [[ "${actual}" == "${expected}" ]]; then
    pass "accepts ${input}"
  else
    fail "accepts ${input}"
  fi
}

assert_rejects() {
  local input="$1"
  if normalize_godot_version "${input}" >/dev/null 2>&1; then
    fail "rejects ${input}"
  else
    pass "rejects ${input}"
  fi
}

assert_normalizes '4.7.1.stable.mono.double.official' '4.7.1.stable.mono.double.official'
assert_normalizes '4.7.1.stable.mono.double.official.a13da4feb' '4.7.1.stable.mono.double.official'
assert_normalizes '4.7.1.stable.mono.double.custom_build' '4.7.1.stable.mono.double.custom_build'
assert_normalizes '4.7.1.stable.mono.double.custom_build.a13da4feb' '4.7.1.stable.mono.double.custom_build'
assert_rejects '4.7.1.stable'
assert_rejects '4.7.1.stable.official.a13da4feb'
assert_rejects '4.7.1.stable.mono.official.a13da4feb'
assert_rejects '4.7.1.stable.double.custom_build.a13da4feb'
assert_rejects '4.7.1.stable.mono.double.custom_build.'
assert_rejects '4.7.1.stable.mono.double.custom_build.ab-bad'
assert_rejects '4.7.1.stable.mono.double.custom_build.ab_bad'
assert_rejects '4.7.1.stable.mono.double.custom_build.ab.bad'
assert_rejects '4.7.0.stable.mono.double.custom_build.a13da4feb'
assert_rejects '4.7.2.stable.mono.double.custom_build.a13da4feb'
assert_rejects '4.8.0.stable.mono.double.custom_build.a13da4feb'
assert_rejects '4.7.1.beta1.mono.double.custom_build.a13da4feb'

if GODOT_BIN="${TEMP_DIR}/missing-godot" resolve_godot_bin >/dev/null 2>&1; then
  fail 'missing GODOT_BIN fails deterministically'
else
  pass 'missing GODOT_BIN fails deterministically'
fi

write_mock_godot() {
  local path="$1"
  local version="$2"
  local headless_exit="$3"
  local log_file="$4"
  local help_output="${5:---build-solutions}"
  {
    printf '%s\n' '#!/usr/bin/env bash'
    printf '%s\n' 'if [[ "$1" == "--version" ]]; then'
    printf '  printf "%%s\\n" "%s"\n' "${version}"
    printf '%s\n' '  exit 0'
    printf '%s\n' 'fi'
    printf '%s\n' 'if [[ "$1" == "--help" ]]; then'
    printf '  printf "%%s\\n" "%s"\n' "${help_output}"
    printf '%s\n' '  exit 0'
    printf '%s\n' 'fi'
    printf 'printf "%%s\\n" "$*" >> "%s"\n' "${log_file}"
    printf 'if [[ "$1" == "--headless" ]]; then exit %s; fi\n' "${headless_exit}"
    printf '%s\n' 'exit 0'
  } >"${path}"
  chmod +x "${path}"
}

MOCK_BIN_DIR="${TEMP_DIR}/bin"
mkdir -p "${MOCK_BIN_DIR}"
printf '%s\n' '#!/usr/bin/env bash' 'if [[ "$1" == "--version" ]]; then echo 10.0.302; exit 0; fi' 'exit 0' >"${MOCK_BIN_DIR}/dotnet"
chmod +x "${MOCK_BIN_DIR}/dotnet"

PATH_GODOT_MONO="${MOCK_BIN_DIR}/godot4-mono"
write_mock_godot "${PATH_GODOT_MONO}" '4.7.1.stable.mono.double.custom_build.a13da4feb' 0 "${TEMP_DIR}/path-mono.log"
if path_result="$(PATH="${MOCK_BIN_DIR}:/usr/bin:/bin" env -u GODOT_BIN bash -c 'source "$1/tools/toolchain.sh"; resolve_godot_bin' bash "${ROOT}")" && [[ "${path_result}" == "${PATH_GODOT_MONO}" ]]; then
  pass 'PATH lookup selects a supported Godot command'
else
  fail 'PATH lookup selects a supported Godot command'
fi

STANDARD_LOG="${TEMP_DIR}/standard.log"
STANDARD_GODOT="${TEMP_DIR}/standard-godot"
write_mock_godot "${STANDARD_GODOT}" '4.7.1.stable.mono.double.custom_build.a13da4feb' 0 "${STANDARD_LOG}" 'standard editor help without .NET options'
if PATH="${MOCK_BIN_DIR}:${PATH}" GODOT_BIN="${STANDARD_GODOT}" "${ROOT}/tools/verify-toolchain.sh" >/dev/null 2>&1; then
  fail 'standard non-.NET editor is rejected when it silently ignores --build-solutions'
else
  pass 'standard non-.NET editor is rejected when it silently ignores --build-solutions'
fi

if PATH="${MOCK_BIN_DIR}:${PATH}" GODOT_BIN="${TEMP_DIR}/missing-godot" "${ROOT}/tools/verify-toolchain.sh" >/dev/null 2>&1; then
  fail 'verification fails when Godot is missing'
else
  pass 'verification fails when Godot is missing'
fi

UNSUPPORTED_LOG="${TEMP_DIR}/unsupported.log"
UNSUPPORTED_GODOT="${TEMP_DIR}/unsupported-godot"
write_mock_godot "${UNSUPPORTED_GODOT}" '4.7.1.stable.mono.double.custom_build.a13da4feb' 1 "${UNSUPPORTED_LOG}"
if PATH="${MOCK_BIN_DIR}:${PATH}" GODOT_BIN="${UNSUPPORTED_GODOT}" "${ROOT}/tools/verify-toolchain.sh" >/dev/null 2>&1; then
  fail 'unsupported Godot executable fails headless verification'
else
  pass 'unsupported Godot executable fails headless verification'
fi

GOOD_LOG="${TEMP_DIR}/good.log"
GOOD_GODOT="${TEMP_DIR}/good-godot"
write_mock_godot "${GOOD_GODOT}" '4.7.1.stable.mono.double.custom_build.a13da4feb' 0 "${GOOD_LOG}"
PATH_GODOT="${MOCK_BIN_DIR}/godot"
write_mock_godot "${PATH_GODOT}" '4.7.0.stable' 0 "${TEMP_DIR}/path.log"
if output="$(PATH="${MOCK_BIN_DIR}:${PATH}" GODOT_BIN="${GOOD_GODOT}" "${ROOT}/tools/verify-toolchain.sh" 2>&1)"; then
  if [[ "${output}" == *'godot: 4.7.1.stable.mono.double.custom_build'* ]] && grep -q -- '--headless' "${GOOD_LOG}"; then
    pass 'GODOT_BIN override is selected and verified'
  else
    fail 'GODOT_BIN override is selected and verified'
  fi
else
  fail 'GODOT_BIN override is selected and verified'
fi

if GODOT_BIN="${GOOD_GODOT}" "${ROOT}/open_godot.sh" --test-argument >/dev/null 2>&1; then
  if grep -q -- '--editor' "${GOOD_LOG}" && grep -q -- '--test-argument' "${GOOD_LOG}"; then
    pass 'Bash launcher opens the override executable'
  else
    fail 'Bash launcher opens the override executable'
  fi
else
  fail 'Bash launcher opens the override executable'
fi

if (( failures > 0 )); then
  exit 1
fi
