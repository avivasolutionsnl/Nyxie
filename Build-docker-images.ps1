Write-Host -ForegroundColor Yellow "Stopping Containers"

docker-compose -f docker-compose.yml -f docker-compose.build.yml down

Write-Host -ForegroundColor Yellow "Starting Containers"

docker-compose -f docker-compose.yml -f docker-compose.build.yml up -d

Write-Host -ForegroundColor Yellow "Installing packages"

docker exec promethium_sitecore_1 powershell c:\sitecore\Install-packages.ps1

Write-Host -ForegroundColor Yellow "Persisting databases"

docker-compose -f docker-compose.yml -f docker-compose.build.yml stop 

docker-compose -f docker-compose.yml -f docker-compose.build.yml up -d mssql

docker exec promethium_mssql_1 powershell c:\Persist-Databases.ps1

docker-compose -f docker-compose.yml -f docker-compose.build.yml stop 

Write-Host -ForegroundColor Yellow "Committing images"

docker commit promethium_mssql_1 avivasolutionsnl.azurecr.io/promethium-mssql:9.1.0-20190528
docker commit promethium_sitecore_1 avivasolutionsnl.azurecr.io/promethium-sitecore:9.1.0-20190528
docker commit promethium_solr_1 avivasolutionsnl.azurecr.io/promethium-solr:9.1.0-20190528
