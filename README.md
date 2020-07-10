# studio-appstore-service
Home for the appstore service used from Studio 2021 onwards for searching and installing plugins.

## Table of contents 

1. [Intro](#intro)
2. [Getting started](#getting-started)
3. [How to configure the service](#service-cofig)

## Intro
This service allow users to create a **Private AppStore** which can be used in SDL Trados Studio to install and update plugins internal plugins which are not released on the AppStore web site.

The service reads a *Json* with a predefined structure with all the plugins which well be available in SDL Trados Studio.

## Getting started
In the **release section** is available an archive with the latest version of the Service. This archive contains following items:
1. The executable file of the service *AppStoreIntegrationService.exe*.
2. Configuration file used by the service, which needs to be filled by each user *appsettings.json*.
3. An folder called *PluginsConfig* which have an example for the json file used by the Service. 
4. An folder called *SettingsFileExample* which have 3 files with the settings needed by the service based on the Deploy Mode.

## How to configure the service
Service can be configured to read the **json file with the plugins info** in 3 ways:
1. From an **Azure Storage** account. That means the json file is stored in a Blob in Azure.
2. Fom an **Local Path** on the server.
3. From an **Network file path**.

This configuration needs to be added in the **appsettings.json** file.

### What information should be added in the appsettings.json file

In the *SettingsFileExample* folder there are 3 files which corresponds to the each deploy option available. Copy the content of the file which corresponds to the deploy option desired and paste it into **appsettings.json** file.

### Azure Blob Deploy mode
In order to use the Azure deploy mode you need to add Azure **Storage Account Name** and **Storage Account Key**. This information set in the **appsettings** file in **"ConfigurationSettings"** object or in the **System environment variables**.

#### System environment variables
If you choose the add the settings in the system variables please remove the **ConfigurationSettings object** from the **appsettings** file if you previously added it and create following **Environment Variables**: 
1. APPSTOREINTEGRATION_BLOBNAME (blob name value should contain only lower case letters)
2. APPSTOREINTEGRATION_CONFIGFILENAME
3. APPSTOREINTEGRATION_STORAGE_ACCOUNTKEY
4. APPSTOREINTEGRATION_STORAGE_ACCOUNTNAME

### ServerFilePath and NetworkFilePath Deploy mode
In order to use one of this deploy option in the **appsettings.json** or in the **Environment Variables**, local folder path" and configuration file name should be added.

**ServerFilePath** config file
```
//Replace with the local path on the server where the json with the plugins info is saved
  "ConfigurationSettings": {
    "LocalFolderPath": "\\PluginsConfig",
	"ConfigFileName": "pluginsConfig.json"
  }
  //If the folder or file does not exist in the specified location they'll be created
  ```
  In the archive "PluginsConfig" folder corresponds to the "LocalFolderPath" set in the example settings file. **If you don't want to set the json file in another path just edit the existing json file from the folder and paste the "ConfigurationSettings" in the existing appsettings.json**. 


