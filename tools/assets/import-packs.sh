#!/bin/bash
# Usage: import-packs.sh
# Copies every manifest pair from download-assets/ into the project: pack-manifest.txt → Assets/_Game/Art/Packs/,
# audio-manifest.txt → Assets/_Game/Audio/. Copy only: downloaded files are untrusted data and nothing in them is
# ever executed. Refuses paths that escape either root.
set -euo pipefail
cd "$(dirname "$0")/../.."
src_root="download-assets"
copied=0
import_manifest() {
  local manifest="$1" dst_root="$2"
  while IFS=$'\t' read -r src dst; do
    [[ -z "${src:-}" || "$src" == \#* ]] && continue
    if [[ -z "${dst:-}" || "$src" == *..* || "$dst" == *..* || "$src" == /* || "$dst" == /* ]]; then
      echo "bad manifest line in $manifest: $src $dst" >&2; exit 1
    fi
    [[ -f "$src_root/$src" ]] || { echo "missing: $src_root/$src" >&2; exit 1; }
    mkdir -p "$(dirname "$dst_root/$dst")"
    cp "$src_root/$src" "$dst_root/$dst"
    copied=$((copied + 1))
  done < "$manifest"
}
import_manifest tools/assets/pack-manifest.txt "Assets/_Game/Art/Packs"
import_manifest tools/assets/audio-manifest.txt "Assets/_Game/Audio"
echo "copied $copied files"
