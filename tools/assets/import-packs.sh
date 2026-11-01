#!/bin/bash
# Usage: import-packs.sh
# Copies every manifest pair from download-assets/ into Assets/_Game/Art/Packs/. Copy only: pack files are
# untrusted data and nothing in them is ever executed. Refuses paths that escape either root.
set -euo pipefail
cd "$(dirname "$0")/../.."
src_root="download-assets"
dst_root="Assets/_Game/Art/Packs"
copied=0
while read -r src dst; do
  [[ -z "${src:-}" || "$src" == \#* ]] && continue
  if [[ -z "${dst:-}" || "$src" == *..* || "$dst" == *..* || "$src" == /* || "$dst" == /* ]]; then
    echo "bad manifest line: $src $dst" >&2; exit 1
  fi
  [[ -f "$src_root/$src" ]] || { echo "missing: $src_root/$src" >&2; exit 1; }
  mkdir -p "$(dirname "$dst_root/$dst")"
  cp "$src_root/$src" "$dst_root/$dst"
  copied=$((copied + 1))
done < tools/assets/pack-manifest.txt
echo "copied $copied files into $dst_root"
