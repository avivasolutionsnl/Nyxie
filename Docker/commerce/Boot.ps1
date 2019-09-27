param(
    [Parameter(Mandatory=$true)]
    [String]$commerceHostname,
    [Parameter(Mandatory=$true)]
    [String]$sitecoreHostname,
    [Parameter(Mandatory=$true)]
    [String]$identityHostname
)

If ((Test-Path C:\Workspace) -eq $False) {
    New-Item -Type Directory c:\Workspace
}
/Scripts/UpdateHostname.ps1 -commerceHostname $commerceHostname -sitecoreHostname $sitecoreHostname -identityHostname $identityHostname
/Scripts/WatchDefaultDirectories.ps1


Write-Host ("{0}: Starting VS 2017 remote debugger..." -f [DateTime]::Now.ToString("HH:mm:ss:fff"))

# VS 2017 debugger: wait 360000 seconds for a user to connect without authentication
& 'C:\Program Files\Microsoft Visual Studio 15.0\Common7\IDE\Remote Debugger\x64\msvsmon.exe' /noauth /anyuser /silent /nosecuritywarn /timeout 3600000

Write-Host ("{0}: Started VS 2017 remote debugger on port 4022." -f [DateTime]::Now.ToString("HH:mm:ss:fff"))
