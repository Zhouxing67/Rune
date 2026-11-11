# Traditional Windows deployment

This fork's installer is named **Rune Community**, with a new stable AppId.
It is independent of the upstream Microsoft Store identity. First release targets
x64-compatible Windows and installs for the current user without elevation.

Install Inno Setup 7 (or 6.3 or newer), and the .NET 10 SDK, then run:

```powershell
powershell -NoProfile -File deploy/installer/build.ps1
```

Use `-IsccPath` to specify ISCC.exe explicitly.
The build script also detects `E:\Environments\Inno Setup\ISCC.exe`.
It also detects `E:\Environments\Inno Setup 7\ISCC.exe`.
Output is under `artifacts/installer`. The version comes from Rune.App.csproj. Publish staging
directories are unique to avoid carrying stale build artifacts into new packages.
Packages are unsigned until a release signing process is configured.

## Install, update, uninstall

Run Setup.exe. Download and run a newer Setup.exe to update the same installation.
Keep AppId and installation scope stable. Older versions are blocked; reinstalling
the same version is allowed. Save documents and close all Rune.exe processes first.
The installer never forcibly terminates a PDF editing session.

Uninstall **Rune Community** from Windows Settings. User data is deliberately
retained; user PDF documents are never removed. Currently the application still
uses `%LOCALAPPDATA%\Rune`, shared with upstream unpackaged builds. The installer
does not migrate or delete that directory. Installation identity is isolated,
but application data isolation remains a separate migration decision.

Shortcuts are installed; this first version does not register PDF associations.
Files removed from future releases require explicit, narrowly scoped removal
rules in the installer; do not recursively delete the application directory.

## Release acceptance checks

On a clean Windows account: install, launch and open a PDF; upgrade from the
previous release and verify settings/signatures; verify an open Rune blocks the
upgrade; test same-version reinstall and downgrade rejection; uninstall and
verify program/shortcuts disappear while user data and PDFs remain. Inspect the
published runtime and licenses. Test x64 emulation separately before advertising
ARM64 compatibility. A successful compilation does not replace these checks.
