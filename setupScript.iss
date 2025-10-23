; Script Inno Setup corrigé pour IPTraySwitcherWPF
; Créé le 20/10/2025

#define MyAppExeName "IPTraySwitcher.exe"
#define MyAppName "IPTraySwitcher"

[Setup]
AppName={#MyAppName}
AppVersion=1.1
AppPublisher=ALC
DefaultDirName={commonpf}\SIIF\{#MyAppName}
DefaultGroupName={#MyAppName}
UninstallDisplayIcon={app}\icon.ico
SetupIconFile=icon.ico
Compression=lzma
SolidCompression=yes
OutputDir=./Setup/
OutputBaseFilename={#MyAppName}_Installer
WizardStyle=modern
LanguageDetectionMethod=none
PrivilegesRequired=admin

[Languages]
Name: "fr"; MessagesFile: "compiler:Languages\French.isl"

[Files]
Source: "bin\Release\net8.0-windows\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net8.0-windows\{#MyAppName}.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net8.0-windows\{#MyAppName}.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net8.0-windows\{#MyAppName}.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net8.0-windows\{#MyAppName}.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net8.0-windows\icon.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\icon.ico"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\icon.ico"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Lancer l'application"; Flags: nowait postinstall skipifsilent runascurrentuser

[Code]
procedure CreateScheduledTask();
var
  PowerShellCmd: string;
  ResultCode: Integer;
begin
  PowerShellCmd :=
    '$action = New-ScheduledTaskAction -Execute ''' + ExpandConstant('{app}\{#MyAppExeName}') + '''; ' +
    '$trigger = New-ScheduledTaskTrigger -AtLogOn; ' +
    '$principal = New-ScheduledTaskPrincipal -UserId $env:USERNAME -RunLevel Highest; ' +
    'Register-ScheduledTask -TaskName "{#MyAppName}_AutoAdmin" -Action $action -Trigger $trigger -Principal $principal -Force';

  if not ShellExec('runas', 'powershell.exe',
    '-NoProfile -ExecutionPolicy Bypass -Command "' + PowerShellCmd + '"',
    '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    MsgBox('Erreur lors de la création de la tâche planifiée. Code: ' + IntToStr(ResultCode),
      mbError, MB_OK);
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
    CreateScheduledTask();
end;


