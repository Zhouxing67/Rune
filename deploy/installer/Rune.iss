#ifndef AppVersion
  #error AppVersion is required
#endif
#ifndef PublishDir
  #error PublishDir is required
#endif
#ifndef OutputDir
  #error OutputDir is required
#endif

[Setup]
; Fork-specific identity. Keep this stable across releases.
AppId={B704D7DB-7779-4858-AB0F-6DB1E504C66C}
AppName=Rune Community
AppVersion={#AppVersion}
DefaultDirName={localappdata}\Programs\RuneCommunity
DefaultGroupName=Rune Community
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir={#OutputDir}
OutputBaseFilename=RuneCommunity-{#AppVersion}-win-x64-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\Rune.exe
CloseApplications=no
RestartApplications=no
SetupLogging=yes

[Tasks]
Name: desktopicon; Description: "Create a desktop shortcut"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Rune Community"; Filename: "{app}\Rune.exe"
Name: "{autodesktop}\Rune Community"; Filename: "{app}\Rune.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\Rune.exe"; Description: "Launch Rune Community"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup(): Boolean;
var
  Installed: String;
  InstalledVersion, NewVersion: Int64;
begin
  Result := True;
  if RegQueryStringValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{B704D7DB-7779-4858-AB0F-6DB1E504C66C}_is1', 'DisplayVersion', Installed) then
    if StrToVersion(Installed, InstalledVersion) and StrToVersion('{#AppVersion}', NewVersion) then
    if ComparePackedVersion(InstalledVersion, NewVersion) > 0 then
    begin
      MsgBox('A newer version is installed. Uninstall it first if you intend to downgrade.', mbError, MB_OK);
      Result := False;
    end;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  Locator, Services, Processes: Variant;
begin
  Result := '';
  try
    Locator := CreateOleObject('WbemScripting.SWbemLocator');
    Services := Locator.ConnectServer('', 'root\CIMV2');
    Processes := Services.ExecQuery('SELECT ProcessId FROM Win32_Process WHERE Name = ''Rune.exe''');
    if Processes.Count > 0 then
      Result := 'Save your documents and close all Rune windows before continuing.';
  except
    Result := 'Unable to check running Rune processes. Close Rune and retry after Windows Management Instrumentation is available.';
  end;
end;
