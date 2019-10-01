param(
    [Parameter(Mandatory=$true)]
    [String]$commerceHostname,
    [Parameter(Mandatory=$true)]
    [String]$identityHostname
)

/Scripts/UpdateHostnames.ps1 -commerceHostname $commerceHostname -identityHostname $identityHostname

If ((Test-Path C:\Workspace) -eq $False) {
    New-Item -Type Directory c:\Workspace
}

$procs = $(Start-Process -FilePath 'C:\Program Files\Microsoft Visual Studio 15.0\Common7\IDE\Remote Debugger\x64\msvsmon.exe' -ArgumentList '/nostatus /silent /noauth /anyuser /nosecuritywarn /timeout 3600000' -PassThru
           Start-Process powershell -ArgumentList '/Scripts/Watch-Directory.ps1 -Path C:\Workspace -Destination c:\inetpub\wwwroot\sitecore' -PassThru)
$procs | Wait-Process
