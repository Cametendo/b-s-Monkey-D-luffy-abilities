#!/usr/bin/env bash
# Builds LuffyAbilities for every supported game version and packs ready-to-install zips into dist/.
#
#   build/build.sh --v1  "<B&S 1.0>/BladeAndSorcery_Data/Managed" \
#                  --u12 "<B&S U12>/BladeAndSorcery_Data/Managed"
#
# Either flag may be omitted to build only that version.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$ROOT/src/LuffyAbilities/LuffyAbilities.csproj"
DIST="$ROOT/dist"
STAGE="$ROOT/build/out"
MOD_FOLDER="LuffyAbilities"
MOD_VERSION="$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' "$PROJECT")"

declare -A MANAGED=()
while [[ $# -gt 0 ]]; do
  case "$1" in
    --v1)  MANAGED[V1]="$2";  shift 2 ;;
    --u12) MANAGED[U12]="$2"; shift 2 ;;
    *) echo "Unknown argument: $1" >&2; exit 1 ;;
  esac
done
[[ ${#MANAGED[@]} -gt 0 ]] || { echo "Pass --v1 and/or --u12 <Managed folder>" >&2; exit 1; }

# GameVersion -> manifest "GameVersion" (the game requires an exact match) and zip label.
game_version() { case "$1" in V1) echo "1.0.0.0" ;; U12) echo "0.12.0.0" ;; esac; }
zip_label()    { case "$1" in V1) echo "BaS-1.0" ;; U12) echo "BaS-U12" ;; esac; }

mkdir -p "$DIST"
for version in "${!MANAGED[@]}"; do
  echo "==> Building for $version"
  dotnet build "$PROJECT" -c Release -nologo -v q \
    -p:GameVersion="$version" -p:BaSManaged="${MANAGED[$version]}"

  out="$STAGE/$version/$MOD_FOLDER"
  rm -rf "$STAGE/$version" && mkdir -p "$out"
  cp "$ROOT/src/LuffyAbilities/bin/Release/$version/LuffyAbilities.dll" "$out/"
  sed -e "s/@MOD_VERSION@/$MOD_VERSION/" -e "s/@GAME_VERSION@/$(game_version "$version")/" \
    "$ROOT/mod/manifest.template.json" > "$out/manifest.json"

  zip_path="$DIST/LuffyAbilities-$MOD_VERSION-$(zip_label "$version").zip"
  rm -f "$zip_path"
  (cd "$STAGE/$version" && zip -qr -X "$zip_path" "$MOD_FOLDER")
  echo "    -> ${zip_path#$ROOT/}"
done
