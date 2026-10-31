#!/bin/bash
# Usage: edit-tests.sh [TestFixtureName]  — runs EditMode tests, prints summary + failures, exit 1 on any failure.
cd "$(dirname "$0")/../.."
args=(command run_tests --mode EditMode --timeout 600 --caller plugin --skill unity-testing --format json)
[ -n "$1" ] && args+=(--filter "$1")
for i in $(seq 1 30); do
  unity "${args[@]}" > /tmp/edit-tests.json 2>&1
  grep -q "Cannot connect" /tmp/edit-tests.json || break
  sleep 3
done
python3 - <<'PY'
import json, os, sys
d = json.load(open('/tmp/edit-tests.json'))
r = (d.get('data') or {}).get('result')
if not r: print('no result:', json.dumps(d)[:400]); sys.exit(1)
s = r['Summary']; print(f"total={s['Total']} passed={s['Passed']} failed={s['Failed']}")
bad = [t for t in r['Results'] if t['Status'] != 'Passed']
for t in bad: print('FAIL', t['FullName'], (t['Message'] or '')[:200])
sys.exit(1 if bad or s['Total'] == 0 else 0)
PY
