# Promethium

[![Build Status](https://dev.azure.com/avivasolutions-public/Hotcakes/_apis/build/status/Continuous%20integration?branchName=master)](https://dev.azure.com/avivasolutions-public/Hotcakes/_build/latest?definitionId=6&branchName=master)

## Prerequisites
- Windows 10 update 1809
- Docker for Windows version >= 18.09.1
- Visual Studio 15.5.3

## Getting started 
- Open a console en navigate to the repository root folder.

- Build the project in Visual Studio and publish the projects to the following folders
Promethium.Serialization --> \build\website
Sitecore.Commerce.Engine --> \build\commerce

- Build Sitecore XC Docker images according to the instructions found here: https://github.com/Sitecore/docker-images
    - Or if you have pre-built Docker images available in a registry, set the `REGISTRY` in (./env)

- Copy your Sitecore license file (license.xml) to the [](./license) folder

- Spin up the environment (make sure you are using Windows and not Linux containers):
```
PS> docker-compose up
```

To set the Docker container service names as DNS names on your host edit your `hosts` file. 
A convenient tool to automatically do this is [whales-names](https://github.com/gregolsky/whales-names).

Initialize your Commerce Engine and setup a Storefront according to the instructions [here](https://github.com/Sitecore/docker-images/tree/master/windows/tests/9.2.x).
> Unselect the Habitat catalog in `Commerce > Catalog Management > Catalogs` before adding a Storefront site

Synchronize the development content by running Unicorn: [http://cm/unicorn.aspx?verb=sync](http://cm/unicorn.aspx?verb=sync).

Fix indexes by:

- Opening the content editor
- Goto the commerce tab
- Delete Data Templates
- Update Data Templates
- Goto control panel and rebuild the `sitecore_master_index` & `sitecore_sxa_master_index`

> If you get an error saying: 'field _indexname' not found: remove files in host [](./data/solr) folder. Restart containers and populate schema.

## Resources
https://sitecoresmurf.wordpress.com/2019/07/18/known-issues-limitations-and-extending-promotion-plugin-in-sitecore-commerce-9/
Out of the box, sitecore doesn't allow boolean values in Conditions.
To change this behavior we made our own version of the BuildRuleSetBlock in which we called our own SitecoreExtensions.ConvertToConditionExtended and SitecoreExtensions.ConvertToActionExtended.