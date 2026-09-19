#!/usr/bin/env bash
# Builds Linux distributables from the published linux-x64 output.
#   - tar.gz (all distros)
#   - .deb  (built with ar + tar; no dpkg tooling required)
#   - .rpm  (requires rpmbuild; set RPMBUILD_HOME to an extracted rpm-build prefix)
set -euo pipefail

VERSION="${VERSION:-0.1.0}"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PUB="$ROOT/dist/linux-x64"
OUT="$ROOT/dist"
PKG="$ROOT/packaging"

if [ ! -x "$PUB/CoreShift" ]; then
  echo "error: publish output not found at $PUB" >&2
  echo "run: dotnet publish src/CoreShift.Game -c Release -r linux-x64 --self-contained true -o dist/linux-x64" >&2
  exit 1
fi

echo "==> tar.gz"
TARDIR="$(mktemp -d)"
cp -r "$PUB" "$TARDIR/CoreShift-$VERSION-linux-x64"
tar -czf "$OUT/CoreShift-$VERSION-linux-x64.tar.gz" -C "$TARDIR" "CoreShift-$VERSION-linux-x64"
rm -rf "$TARDIR"

echo "==> .deb"
STAGE="$(mktemp -d)"
mkdir -p "$STAGE/usr/lib/coreshift" "$STAGE/usr/bin" "$STAGE/usr/share/applications"
cp -r "$PUB/." "$STAGE/usr/lib/coreshift/"
install -Dpm 0755 "$PKG/coreshift.launcher" "$STAGE/usr/bin/coreshift"
install -Dpm 0644 "$PKG/coreshift.desktop" "$STAGE/usr/share/applications/coreshift.desktop"

CTRL="$(mktemp -d)"
SIZE_KB="$(du -sk "$STAGE" | cut -f1)"
cat > "$CTRL/control" <<EOF
Package: coreshift
Version: $VERSION
Section: games
Priority: optional
Architecture: amd64
Maintainer: DCIT 318 Team <team@example.invalid>
Installed-Size: $SIZE_KB
Description: CoreShift: Chrono-Survival
 2D top-down roguelike twin-stick survival game with neon presentation,
 procedural audio, status effects, crits, pickups and permanent progression.
 Bundles the self-contained .NET runtime and raylib.
EOF

DEBWORK="$(mktemp -d)"
tar -C "$CTRL" --owner=0 --group=0 -czf "$DEBWORK/control.tar.gz" .
tar -C "$STAGE" --owner=0 --group=0 -czf "$DEBWORK/data.tar.gz" .
printf '2.0\n' > "$DEBWORK/debian-binary"
( cd "$DEBWORK" && ar rc "$OUT/CoreShift-$VERSION-linux-amd64.deb" debian-binary control.tar.gz data.tar.gz )
rm -rf "$STAGE" "$CTRL" "$DEBWORK"

echo "==> .rpm"
RPMBUILD_BIN="$(command -v rpmbuild || true)"
RPM_DEFINES=()
if [ -z "$RPMBUILD_BIN" ] && [ -n "${RPMBUILD_HOME:-}" ] && [ -x "$RPMBUILD_HOME/usr/bin/rpmbuild" ]; then
  RPMBUILD_BIN="$RPMBUILD_HOME/usr/bin/rpmbuild"
  # Merge the system rpm config with the extracted rpm-build scripts/macros.
  MERGED="$(mktemp -d)"
  cp -r /usr/lib/rpm/. "$MERGED/" 2>/dev/null || true
  cp -r "$RPMBUILD_HOME/usr/lib/rpm/." "$MERGED/" 2>/dev/null || true
  export RPM_CONFIGDIR="$MERGED"
  RPM_DEFINES=(--define "_rpmconfigdir $MERGED")
fi

if [ -z "$RPMBUILD_BIN" ]; then
  echo "   rpmbuild not found; skipping .rpm (set RPMBUILD_HOME to an extracted rpm-build prefix)"
else
  TOP="$(mktemp -d)"
  mkdir -p "$TOP/BUILD" "$TOP/RPMS" "$TOP/SOURCES" "$TOP/SPECS" "$TOP/SRPMS"
  cp -r "$PUB" "$TOP/SOURCES/payload"
  cp "$PKG/coreshift.launcher" "$PKG/coreshift.desktop" "$TOP/SOURCES/"
  cp "$PKG/coreshift.spec" "$TOP/SPECS/"
  if "$RPMBUILD_BIN" -bb "$TOP/SPECS/coreshift.spec" --define "_topdir $TOP" "${RPM_DEFINES[@]}" >/tmp/opencode/rpmbuild.log 2>&1; then
    find "$TOP/RPMS" -name '*.rpm' -exec cp {} "$OUT/CoreShift-$VERSION-linux-x86_64.rpm" \;
  else
    echo "   rpmbuild failed; see /tmp/opencode/rpmbuild.log"
    tail -20 /tmp/opencode/rpmbuild.log
  fi
  rm -rf "$TOP"
fi

echo "==> done"
ls -la "$OUT" | grep -E '\.(tar\.gz|deb|rpm)$' || true
