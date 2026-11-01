#!/bin/bash
# Usage: build.sh [--tests] [name ...]
#   --tests   run the Python tests in tools/blender/tests inside Blender
#   name      build only these generators (props/<name>.py, or "fracture", "cat"); default builds everything
# Generated files land in Assets/_Game/Art/Generated/. Blender runs headless with factory settings.
set -euo pipefail
cd "$(dirname "$0")/../.."
blender="${BLENDER:-/Applications/Blender.app/Contents/MacOS/Blender}"
out="Assets/_Game/Art/Generated"
run() {
  local log status=0
  log=$(mktemp)
  "$blender" --background --factory-startup --python-exit-code 1 --python "$1" -- "${@:2}" >"$log" 2>&1 || status=$?
  grep -E "^(OK|FAIL|ERROR|Traceback|  File|[A-Za-z]*Error|wrote|test_|Ran )" "$log" || true
  rm -f "$log"
  return "$status"
}

if [[ "${1:-}" == "--tests" ]]; then
  run tools/blender/tests/run_tests.py
  exit $?
fi

mkdir -p "$out"
targets=("$@")
if [[ ${#targets[@]} -eq 0 ]]; then
  targets=()
  for f in tools/blender/props/*.py; do targets+=("$(basename "$f" .py)"); done
  [[ -f tools/blender/fracture_sets.py ]] && targets+=(fracture)
  [[ -f tools/blender/cat/build_cat.py ]] && targets+=(cat)
fi
for name in "${targets[@]}"; do
  case "$name" in
    fracture) run tools/blender/fracture_sets.py --out "$out" ;;
    cat)      run tools/blender/cat/build_cat.py --out "$out" ;;
    cat-ai)   run tools/blender/cat/cleanup.py --out "$out" ;;
    *)        run "tools/blender/props/$name.py" --out "$out" ;;
  esac
done
