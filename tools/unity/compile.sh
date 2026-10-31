#!/bin/bash
# Recompile and print the final status line ("completed", "completed, N errors: …", or "up_to_date").
cd "$(dirname "$0")/../.."
unity command recompile --caller plugin --skill unity-cli >/dev/null 2>&1
for i in $(seq 1 60); do
  s=$(unity command recompile_status --caller plugin --skill unity-cli 2>&1 | tail -1)
  case "$s" in *completed*|*up_to_date*|*failed*) break;; esac
  sleep 3
done
echo "$s" | cut -f3 | cut -c1-800
case "$s" in *error*) exit 1;; esac
