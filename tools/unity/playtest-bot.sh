#!/bin/bash
# Usage: [SMASH_BOT_ROOMS="2 3"] [SMASH_BOT_LEVELS="26,27"] playtest-bot.sh — runs the PlaytestBot (spec §6.3) headless
# with the Editor CLOSED and writes docs/playtest/room<N>-report.{json,md} for each room (default: all five). With
# SMASH_BOT_LEVELS, only those global level numbers are re-played; the other rows are kept.
cd "$(dirname "$0")/../.."
version=$(sed -n 's/^m_EditorVersion: //p' ProjectSettings/ProjectVersion.txt)
editor="/Applications/Unity/Hub/Editor/$version/Unity.app/Contents/MacOS/Unity"
results="$PWD/Logs/bot-results.xml"
rooms="${SMASH_BOT_ROOMS:-1 2 3 4 5}"
filter=$(for r in $rooms; do printf 'Game\\.Tests\\.PlaytestBot\\.PlayRoom\\(%s\\);' "$r"; done)
rm -f "$results"
"$editor" -batchmode -projectPath "$PWD" -logFile Logs/bot-batch.log -runTests -testPlatform PlayMode \
  -testFilter "${filter%;}" -testCategory Bot -testResults "$results" >/dev/null 2>&1
grep -h "\[Bot\]" Logs/bot-batch.log | grep -v "Debug.Log" | sed 's/^.*\[Bot\]/[Bot]/'
python3 - "$results" <<'PY'
import sys, xml.etree.ElementTree as ET
try: root = ET.parse(sys.argv[1]).getroot()
except (OSError, ET.ParseError) as e: print('no results:', e, '(see Logs/bot-batch.log)'); sys.exit(1)
print(f"bot run: total={root.get('total')} passed={root.get('passed')} failed={root.get('failed')}")
sys.exit(0 if int(root.get('total', 0)) > 0 and root.get('failed') == '0' else 1)
PY
for r in $rooms; do cat "docs/playtest/room$r-report.md" 2>/dev/null; done
