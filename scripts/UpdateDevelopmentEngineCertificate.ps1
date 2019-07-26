Param(
  [string]$engineCertName = "storefront.engine"
)

$certificateStore = "cert:\\LocalMachine\\My"
$configPath = "..\src\Sitecore.Commerce.Engine\wwwroot\config.json"

$certificates = Get-ChildItem -Path $certificateStore -DnsName $engineCertName 

if ($certificates.Length -gt 0) {
    Write-Host "Updating certificate thumbprint" -ForegroundColor Green

    $certificate = $certificates[0]

    $originalJson = Get-Content $configPath -Raw  | ConvertFrom-Json
    $originalJson.Certificates.Certificates[0].Thumbprint = $certificate.Thumbprint
    $originalJson | ConvertTo-Json -Depth 100 -Compress | set-content $configPath				
}
else {
    Write-Host "$engineCertName not found" -ForegroundColor Red   
}