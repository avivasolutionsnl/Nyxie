$filesPath = "/files-mount"

# Use Commerce SIF modules to install packages
[Environment]::SetEnvironmentVariable('PSModulePath', $env:PSModulePath + ';' + "$env:INSTALL_TEMP/Modules");

Install-SitecoreConfiguration -Path '/sitecore/install-packages.json' `
    -StorefrontPackageFullPath "$filesPath/Sitecore Commerce Experience Accelerator Storefront 2.0.181.zip" `
    -StorefrontThemesPackageFullPath "$filesPath/Sitecore Commerce Experience Accelerator Storefront Themes 2.0.181.zip" `
    -HabitatImagesPackageFullPath "$filesPath/Sitecore.Commerce.Habitat.Images-1.0.0.zip" 