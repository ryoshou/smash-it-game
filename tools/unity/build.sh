#!/bin/bash
# Usage: build.sh ios — development Xcode project in Builds/iOS. (Android is deferred until release.)
# Editor open: builds through the bridge. Editor closed: headless `unity run -executeMethod`.
cd "$(dirname "$0")/../.."
case "$1" in
  ios) method=Game.Editor.BuildScripts.BuildIos; target=iOS; output=Builds/iOS ;;
  android) echo "Android builds are deferred until release." >&2; exit 2 ;;
  *) echo "usage: $0 ios" >&2; exit 2 ;;
esac
if unity command editor_status --caller plugin --skill unity-cli 2>/dev/null | grep -q '"status":"ready"'; then
  script=$(mktemp -t build).cs
  printf '%s();\nreturn "built";\n' "$method" > "$script"
  unity command eval_file --file "$script" --timeout 1800 --caller plugin --skill unity-cli | tail -1 | cut -c1-400
  rm -f "$script"
else
  unity run "$PWD" --no-tail -l Logs/build-$1.log -- -buildTarget "$target" -executeMethod "$method" || {
    grep -E "error|\[BuildScripts\]" Logs/build-$1.log | tail -20; exit 1; }
  grep "\[BuildScripts\]" Logs/build-$1.log | tail -1
fi
[ -d "$output" ] && echo "output: $output ($(du -sh "$output" | cut -f1))"
