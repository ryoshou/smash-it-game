#!/bin/bash
# Usage: run-method.sh Game.Editor.Class.Method — runs an editor builder headless (Editor CLOSED).
# Prints the method's error lines on failure; exit 1 when Unity reports an error or exception.
cd "$(dirname "$0")/../.."
[ -n "$1" ] || { echo "usage: $0 <Namespace.Class.Method>" >&2; exit 2; }
log="Logs/method-$(echo "$1" | tr -c 'A-Za-z0-9' '_').log"
unity run "$PWD" --no-tail -l "$log" -- -executeMethod "$1" >/dev/null 2>&1
status=$?
if [ $status -ne 0 ] || grep -qE "Exception|error CS|Art check failed" "$log"; then
  grep -E "Exception|error CS|Art check failed|Error" "$log" | grep -v "^UnityEngine" | head -20
  exit 1
fi
echo "ran $1"
