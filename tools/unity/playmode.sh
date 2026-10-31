#!/bin/bash
# Fresh domain reload → settle → run PlayMode tests async → poll with editor focus. Exit 1 on failure.
cd "$(dirname "$0")/../.."
unity command eval_file --file tools/unity/eval/reload.cs --caller plugin --skill unity-cli >/dev/null 2>&1
sleep 5
for i in $(seq 1 40); do unity command editor_status --caller plugin --skill unity-cli 2>&1 | grep -q '"domainReloadInProgress":false' && break; sleep 2; done
sleep 30
unity command run_tests --mode PlayMode --async_tests true --caller plugin --skill unity-testing > /tmp/pm-start.txt 2>&1
for i in $(seq 1 80); do
  unity command editor_focus --caller plugin --skill unity-cli >/dev/null 2>&1
  unity command test_status --caller plugin --skill unity-testing --format json > /tmp/pm-status.json 2>&1
  python3 -c "
import json,sys;d=json.load(open('/tmp/pm-status.json'));r=(d.get('data') or {}).get('result') or {}
sys.exit(0 if r.get('status') in ('completed','failed') else 1)" && break
  sleep 5
done
python3 - <<'PY'
import json, sys
r = (json.load(open('/tmp/pm-status.json')).get('data') or {}).get('result') or {}
s = r.get('summary') or {}
print(f"status={r.get('status')} total={s.get('total')} passed={s.get('passed')} failed={s.get('failed')}")
for t in r.get('results') or []:
    if t['Status'] != 'Passed': print('FAIL', t['FullName'], (t['Message'] or '')[:300])
sys.exit(0 if s.get('total') and not s.get('failed') else 1)
PY
