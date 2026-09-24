#!/usr/bin/env bash
# Builds CoreShift.app from a published macOS output and packages it as .dmg and .zip.
# Requires macOS tooling (hdiutil, ditto, codesign) — run on a macOS runner or Mac.
#
# usage: make-macos-app.sh <osx-arm64|osx-x64> [version]
set -euo pipefail

RID="${1:?usage: make-macos-app.sh <osx-arm64|osx-x64> [version]}"
VERSION="${2:-0.1.0}"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PUB="$ROOT/dist/$RID"
APP="$ROOT/dist/CoreShift.app"
ARCH="${RID#osx-}"
BUNDLE_ID="com.niicommey01.coreshift"

if [ ! -x "$PUB/CoreShift" ]; then
  echo "error: $PUB/CoreShift not found; publish first" >&2
  exit 1
fi

rm -rf "$APP"
mkdir -p "$APP/Contents/MacOS" "$APP/Contents/Resources"
cp -R "$PUB/." "$APP/Contents/MacOS/"

cat > "$APP/Contents/Info.plist" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleName</key><string>CoreShift</string>
  <key>CFBundleDisplayName</key><string>CoreShift: Chrono-Survival</string>
  <key>CFBundleIdentifier</key><string>${BUNDLE_ID}</string>
  <key>CFBundleVersion</key><string>${VERSION}</string>
  <key>CFBundleShortVersionString</key><string>${VERSION}</string>
  <key>CFBundlePackageType</key><string>APPL</string>
  <key>CFBundleExecutable</key><string>CoreShift</string>
  <key>CFBundleInfoDictionaryVersion</key><string>6.0</string>
  <key>LSMinimumSystemVersion</key><string>11.0</string>
  <key>NSHighResolutionCapable</key><true/>
  <key>NSPrincipalClass</key><string>NSApplication</string>
</dict>
</plist>
PLIST

# Sign nested Mach-O binaries first, then the bundle. Signing the dylib is required on
# Apple Silicon (an unsigned native library is killed by the OS). --deep is deprecated and
# does not reliably sign nested libraries, so we sign each one explicitly.
while IFS= read -r -d '' lib; do
  codesign --force --sign - "$lib" >/dev/null 2>&1 || echo "warn: codesign failed for $lib"
done < <(find "$APP/Contents/MacOS" -name '*.dylib' -print0)
codesign --force --sign - "$APP/Contents/MacOS/CoreShift" >/dev/null 2>&1 || echo "warn: codesign executable failed"
codesign --force --sign - "$APP" >/dev/null 2>&1 || echo "warn: codesign bundle failed"

hdiutil create -volname "CoreShift" -srcfolder "$APP" -ov -format UDZO \
  "$ROOT/dist/CoreShift-$VERSION-macos-$ARCH.dmg"
ditto -c -k --keepParent "$APP" "$ROOT/dist/CoreShift-$VERSION-macos-$ARCH.zip"
