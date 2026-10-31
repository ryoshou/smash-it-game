#!/bin/bash
# Usage: batch-tests.sh editmode|playmode [--coverage]
# Runs tests headless with the Editor CLOSED (calls the Unity binary directly: `unity run` adds -quit, which
# exits before the test runner starts). Prints "total= passed= failed=" + failures; exit 1 on any failure.
cd "$(dirname "$0")/../.."
case "$1" in
  editmode) platform=EditMode ;;
  playmode) platform=PlayMode ;;
  *) echo "usage: $0 editmode|playmode [--coverage]" >&2; exit 2 ;;
esac
version=$(sed -n 's/^m_EditorVersion: //p' ProjectSettings/ProjectVersion.txt)
editor="/Applications/Unity/Hub/Editor/$version/Unity.app/Contents/MacOS/Unity"
results="Logs/$1-results.xml"
args=(-batchmode -projectPath "$PWD" -logFile "Logs/$1-batch.log" -runTests -testPlatform "$platform" -testResults "$PWD/$results")
[ "$2" = "--coverage" ] && args+=(-enableCodeCoverage -coverageResultsPath CodeCoverage
  -coverageOptions "generateHtmlReport;generateAdditionalMetrics;assemblyFilters:+Game.Runtime")
rm -f "$results"
"$editor" "${args[@]}" >/dev/null 2>&1
python3 - "$results" <<'PY'
import sys, xml.etree.ElementTree as ET
try: root = ET.parse(sys.argv[1]).getroot()
except (OSError, ET.ParseError) as e: print('no results:', e, '(see Logs/*-batch.log)'); sys.exit(1)
print(f"total={root.get('total')} passed={root.get('passed')} failed={root.get('failed')}")
for case in root.iter('test-case'):
    if case.get('result') != 'Passed':
        msg = case.find('failure/message')
        print('FAIL', case.get('fullname'), ((msg.text if msg is not None else '') or '')[:300])
sys.exit(0 if int(root.get('total', 0)) > 0 and root.get('failed') == '0' else 1)
PY
