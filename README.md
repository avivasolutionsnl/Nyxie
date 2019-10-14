# Promethium

## Prerequisites
- Windows 10 update 1809
- Docker for Windows version >= 18.09.1
- Visual Studio 15.5.3

## Getting started 
- Open a console en navigate to the repository root folder.

- Build the project in Visual Studio and publish the projects to the following folders
Promethium.Serialization --> \build\website
Sitecore.Commerce.Engine --> \build\commerce

- Copy your Sitecore license file (license.xml) to the .\license folder

- Login in to the Docker repository using your Aviva credentials:
```
PS> az acr login --name avivasolutionsnl
```

- Spin up the environment, make sure you are using windows and not linux containers:
```
PS> docker-compose up
```

To set the Docker container service names as DNS names on your host edit your `hosts` file. 
A convenient tool to automatically do this is [whales-names](https://github.com/gregolsky/whales-names).

Synchronize the development content by running Unicorn: [http://sitecore/unicorn.aspx?verb=sync](http://sitecore/unicorn.aspx?verb=sync).

Fix indexes by:

- Opening the content editor
- Goto the commerce tab
- Delete Data Templates
- Update Data Templates
- Goto control panel and rebuild the `sitecore_master_index` & `sitecore_sxa_master_index`

> If you get an error saying: 'field _indexname' not found: remove files in host \cores folder. Restart containers and populate schema.

## Build Promethium docker images
- Run the docker script `.\Build-docker-images.ps1 ` 
- Publish the docker images, for example:

```
docker push avivasolutionsnl.azurecr.io/promethium-sitecore:9.1.0-20190528
docker push avivasolutionsnl.azurecr.io/promethium-solr:9.1.0-20190528
docker push avivasolutionsnl.azurecr.io/promethium-mssql:9.1.0-20190528
```

## Resources

https://sitecoresmurf.wordpress.com/2019/07/18/known-issues-limitations-and-extending-promotion-plugin-in-sitecore-commerce-9/
Out of the box, sitecore doesn't allow boolean values in Conditions.
To change this behavior we made our own version of the BuildRuleSetBlock in which we called our own SitecoreExtensions.ConvertToConditionExtended and SitecoreExtensions.ConvertToActionExtended.