Name:           coreshift
Version:        0.1.0
Release:        1
Summary:        CoreShift: Chrono-Survival - roguelike twin-stick survival game

License:        MIT
URL:            https://example.invalid/coreshift
BuildArch:      x86_64
AutoReqProv:    no

%global debug_package %{nil}
%global __os_install_post %{nil}
%global __arch_install_post %{nil}

%description
CoreShift: Chrono-Survival is a 2D top-down roguelike survival game. Fight scaling waves,
level up and pick randomized upgrades, and spend credits on permanent progression. Includes
a neon follow-camera presentation, procedural audio, status effects, crits, and pickups.

This package bundles the self-contained .NET runtime and the raylib native library, so no
additional dependencies are required.

%prep
# Nothing to prepare; payload is staged in %{_sourcedir}/payload

%install
rm -rf %{buildroot}
mkdir -p %{buildroot}/usr/lib/coreshift
cp -r %{_sourcedir}/payload/. %{buildroot}/usr/lib/coreshift/
install -Dpm 0755 %{_sourcedir}/coreshift.launcher %{buildroot}/usr/bin/coreshift
install -Dpm 0644 %{_sourcedir}/coreshift.desktop %{buildroot}/usr/share/applications/coreshift.desktop

%files
/usr/lib/coreshift
/usr/bin/coreshift
/usr/share/applications/coreshift.desktop
