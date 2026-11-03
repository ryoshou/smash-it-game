#!/bin/bash
# Usage: [SMASH_BOT_LEVELS="6,7"] playtest-bot.sh — runs the PlaytestBot (spec §6.3) headless with the Editor CLOSED and
# writes docs/playtest/room1-report.{json,md}. With SMASH_BOT_LEVELS, only those level numbers are re-played.
cd "$(dirname "$0")/../.."
version=$(sed -n 's/^m_EditorVersion: //p' ProjectSettings/ProjectVersion.txt)
editor="/Applications/Unity/Hub/Editor/$version/Unity.app/Contents/MacOS/Unity"
results="$PWD/Logs/bot-results.xml"
rm -f "$results"
"$editor" -batchmode -projectPath "$PWD" -logFile Logs/bot-batch.log -runTests -testPlatform PlayMode \
  -testFilter "Game.Tests.PlaytestBot" -testCategory Bot -testResults "$results" >/dev/null 2>&1
grep -h "\[Bot\]" Logs/bot-batch.log | grep -v "Debug.Log" | sed 's/^.*\[Bot\]/[Bot]/'
python3 - "$results" <<'PY'
import sys, xml.etree.ElementTree as ET
try: root = ET.parse(sys.argv[1]).getroot()
except (OSError, ET.ParseError) as e: print('no results:', e, '(see Logs/bot-batch.log)'); sys.exit(1)
print(f"bot run: total={root.get('total')} passed={root.get('passed')} failed={root.get('failed')}")
sys.exit(0 if int(root.get('total', 0)) > 0 and root.get('failed') == '0' else 1)
PY
cat docs/playtest/room1-report.md 2>/dev/null
