# studio-appstore-service
Home for the appstore service used from Studio 2021 onwards for searching and installing plugins.

## Table of contents 

1. [Intro](#intro)
2. [Getting started](#getting-started)

## Intro
This service allow users to create a **Private AppStore** which can be used in SDL Trados Studio to install and update plugins internal plugins which are not released on the AppStore web site.

The service reads a *Json* with a predefined structure with all the plugins which well be available in SDL Trados Studio.

## Getting started
In the **release section** is available an archive with the latest version of the Service. This archive contains following items:
1. The executable file of the service *AppStoreIntegrationService.exe*.
2. Configuration file used by the service, which needs to be filled by each user *appsettings.json*.
3. An folder called *PluginsConfig* which have an example for the json file used by the Service. 
4. An folder called *SettingsFileExample* which have 3 example files with the settings needed by the service based on the Deploy Mode. 
