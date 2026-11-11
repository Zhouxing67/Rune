# Rune — Project reference

Current source version: **0.8.1**. MSIX identity version: **0.8.1.0**.
Updated 2026-11-11. These versions describe source, not Store publication status.

## Architecture

- `Rune.App`: WinUI 3 shell, tabs, PDF viewer, editing controls, printing and sharing.
- `Rune.Engine`: PDF document services, render scheduling, layout, search and local state.
- `Rune.PdfiumInterop`: PDFium P/Invoke bindings and streamed file access.
- `Rune.Tests`: xUnit tests using generated PDFs in `tests/corpus`.

The projects target .NET 10. The app references Windows App SDK subpackages
and Win2D. PDFium access must be serialized; rendering uses a single scheduler
and a bounded tile cache. Preserve native document, page and callback lifetimes.

## Build and verify

On Windows with the .NET 10 SDK and access to NuGet:

```powershell
dotnet restore src/Rune.App/Rune.App.csproj -p:Platform=x64
dotnet build src/Rune.App/Rune.App.csproj -c Release -p:Platform=x64
dotnet test tests/Rune.Tests/Rune.Tests.csproj -c Release
```

The unpackaged self-contained executable is at
`src/Rune.App/bin/x64/Release/net10.0-windows10.0.19041.0/win-x64/Rune.exe`.
Pass a PDF path as its first argument. `--page N` and `--zoom Z` are optional.
For ARM64, build the app project with `-p:Platform=ARM64`.
Do not pass `Platform=x64` to `Rune.slnx`; its solution configuration does not
currently define that platform. CI builds the app project for both architectures.

## Behavior and limits

See README.md for supported features and shortcuts. Text box resizing reflows
words within the new box without changing the font size. Streamed PDF input is
limited to 4 GiB minus one byte. Forms use AcroForm, not XFA; form JavaScript
is not executed. Visible signatures are supported, but cryptographic signature
validation is not. Placed pictures are limited to 1024 pixels on the longest edge.

## Resources and maintenance

`assets/branding/rune-pdf-icon.png` is the current red PDF icon source.
Run `powershell -NoProfile -File tools/gen-icon.ps1` to regenerate `RunePdf.ico`
and the MSIX scale/targetsize images. Windows uses those variants implicitly;
they are not unused merely because their exact filenames are absent from XAML.

`docs/store-screenshots` contains images for the README and Store listing.
`test_files` contains local manual-test documents, not automated test fixtures;
do not publish them as demo assets. Keep LICENSE, THIRD-PARTY-NOTICES.md and
third_party/WindowsAppSDK-NOTICE.txt with distributed binaries.

Before release, keep the project and package manifest versions aligned, build
both architectures, run the tests, verify startup with a corpus PDF, and run
`tools/check-package.ps1` on the actual release package. Local validation is not
proof that a Store package has been built, submitted or published.
