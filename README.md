# studio-appstore-service
This is the home for the appstore service used by SDL Trados Studio 2021 onwards for searching and installing plugins.

## Table of contents 

1. [Intro](#intro)
2. [Getting started](#getting-started)
3. [How to configure the service](#how-to-configure-the-service)
4. [How to run the service](#how-to-run-the-service)
5. [How to host a service on IIS](#how-to-host-a-service-on-iis)

## Intro
This service allows users to create a **Private AppStore** which can be used in SDL Trados Studio to install and update plugins whether they are released on the AppStore web site or not.

The service reads a *Json* with a predefined structure containing details for all the plugins which will be made available in SDL Trados Studio.

## Getting started
In the **release section** there is an archive available for the latest version of the Service. This archive contains the following items:
1. The executable file of the service *AppStoreIntegrationService.exe*.
2. A configuration file used by the service which needs to be completed by each user *appsettings.json*.
3. A folder called *PluginsConfig* which contains an example of the json file used by the Service. 
4. A folder called *SettingsFileExample* which has 3 files containing the settings needed by the service based on the Deploy Mode.

For the latest release of the service, **SqlLocalDB** must be installed on the machine. This  dependency was introduced when we added support for Authentication/Authorization.
This dependency can be downloaded from: https://download.microsoft.com/download/7/c/1/7c14e92e-bdcb-4f89-b7cf-93543e7112d1/SqlLocalDB.msi

## How to configure the service
The service can be configured to read the **json file with the plugins info** in 3 ways:
1. From an **Azure Storage** account. That means the json file is stored in a Blob in Azure.
2. From a **Local Path** on the server.
3. From a **Network file path**.

The configuration to support this needs to be added in the **appsettings.json** file.

### What information should be added in the appsettings.json file

In the *SettingsFileExample* folder there are 3 files which correspond to each deploy option available. Copy the content of the file which corresponds to the desired deploy option and paste it into the **appsettings.json** file.

### Azure Blob Deploy mode
In order to use the Azure deploy mode you need to add the Azure **Storage Account Name** and the **Storage Account Key**. This information needs to be written into the **"ConfigurationSettings"** object in the **appsettings** file or in the **System environment variables**.

#### System environment variables
If you choose the add the settings in the system variables please make sure you remove the **ConfigurationSettings object** from the **appsettings** file (if you had added it) and create the following **Environment Variables**: 
```
1. APPSTOREINTEGRATION_BLOBNAME
2. APPSTOREINTEGRATION_CONFIGFILENAME
3. APPSTOREINTEGRATION_STORAGE_ACCOUNTKEY
4. APPSTOREINTEGRATION_STORAGE_ACCOUNTNAME
5. APPSTOREINTEGRATION_MAPPINGFILENAME (Not mandatory. This variable is used for Name mapping feature)
```
**Blob name rules** 

Azure has following validation rules for Blob name:
```
- It should contain only lower case letters, letters and digits. No special characters are allowed.
- Minimum required lenght is 3
```
Appstore service by default change the blob name to follow the Azure requirements:
```
1. If the blob name is not provider the default name used is **defaultblobname**.
2. If the choosed name contains special characters they'll stripped out.
3. Blob name will be converted to lower case.
4. If the name doesn't have more that 3 letters we'll add "appstore" to choosen name.
For example: 
Name selected: private_appstore_Blob this name will be transformed by the service in -> privateappstoreblob
Name selected: PRIVATEstore -> privatestore
Name selected: abc -> abcappstore
```

The **ConnectionStrings** section of the **appsettings** can also be set as an environment variable.

### ServerFilePath and NetworkFilePath Deploy mode
In order to use one of these deploy options in the **appsettings.json**, or in the **Environment Variables**, the local folder path" and configuration file name should be added.

```
//Replace with the local path on the server where the json with the plugins info is saved
  "ConfigurationSettings": {
	"LocalFolderPath": "\\PluginsConfig",
	"ConfigFileName": "pluginsConfig.json",
	"MappingFileName": "mappingFile.json" // json file where the old name of the plugin is mapped with the new one (Used in Trados Studio to avoid duplicated plugins after a plugin name is changed)
  }
  //If the folder or file does not exist in the specified location they'll be created
  ```
**ServerFilePath** config file
 
In the archive, the "PluginsConfig" folder corresponds to the "LocalFolderPath" set in the example settings file. **If you don't want to set the json file in another path just edit the existing json file from the folder and paste the "ConfigurationSettings" into the existing appsettings.json**.
  
  **NetworkFilePath** config file
  
The steps described in the Server File Path case above apply to the Network deploy case, however the value used for the "LocalFilePath" property should point to a network file path.
Example: "\\\\Networkname\\Folder\\PluginsConfig"

If you don't want to specify this property in the file the following properties should be added in the **System environment variables**:

 ```
1. APPSTOREINTEGRATION_LOCAL_FOLDERPATH
2. APPSTOREINTEGRATION_CONFIGFILENAME
```

## How to run the service

- Open a comand prompt window and navigate to the folder where the server is located. 
- Type the following command **AppStoreIntegrationService.exe**


**If you get an HTTPS error message in the console when trying to start the service**

- execute ```dotnet dev-certs https --trust```

- if that command is not working either, check to see if you've got the .NET Core installed by executing ```dotnet --info``` which should show you details regarding the .NET installation on your PC. You should have .NET Core 3.1 SDK or greater, installed.

- if you don't, then go to https://dotnet.microsoft.com/download#windowsvs2015 and install it
- restart the PC and execute ```dotnet dev-certs https --trust``` again
- run the service again

## How to host a service on IIS
A detailed explanation on how to host on IIS can be found [here](https://www.guru99.com/deploying-website-iis.html).

When hosting on IIS, change the connection string from 
```
AppStoreIntegrationServiceContextConnection": "Server=(localdb)\\mssqllocaldb;Database=AppStoreIntegrationServiceAuthentication;Trusted_Connection=True;MultipleActiveResultSets=true"
```
to
```
"AppStoreIntegrationServiceContextConnection": "Server=localhost\\SQLEXPRESS;Database=AppStoreIntegrationAuthentication;Trusted_Connection=True;"
```
Make sure you have installed SQL Express on your server: https://www.microsoft.com/en-us/sql-server/sql-server-downloads

