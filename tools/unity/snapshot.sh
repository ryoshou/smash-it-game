#!/bin/bash
# Usage: snapshot.sh out.png [level-index] [--break] [--cat Take:seconds] — renders the Game scene camera (portrait
# 1170x2532) with that level's props placed; --break stages a mug mid-shatter with effects; --cat poses the cat from
# one of its takes (Idle, Aim, Release, Celebrate, Annoyed). Editor CLOSED (headless).
cd "$(dirname "$0")/../.."
[ -n "$1" ] || { echo "usage: $0 out.png [level] [--break] [--cat Take:seconds]" >&2; exit 2; }
out=$(cd "$(dirname "$1")" && pwd)/$(basename "$1")
args=(-executeMethod Game.Editor.Snapshot.FromCommandLine -snapshotOut "$out" -snapshotLevel "${2:-0}")
shift; shift
while [ $# -gt 0 ]; do
  case "$1" in
    --break) args+=(-snapshotBreak) ;;
    --cat) args+=(-snapshotCatPose "$2"); shift ;;
  esac
  shift
done
unity run "$PWD" --no-tail -l Logs/snapshot.log -- "${args[@]}" >/dev/null 2>&1
grep -E "\[Snapshot\]|Exception" Logs/snapshot.log | head -5
